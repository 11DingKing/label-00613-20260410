using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PdfToOfd.Api.Data;
using PdfToOfd.Api.Models;

namespace PdfToOfd.Api.Services;

public class ConversionBackgroundService : BackgroundService
{
    private readonly IConversionTaskQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConversionBackgroundService> _logger;

    public ConversionBackgroundService(
        IConversionTaskQueue taskQueue,
        IServiceProvider serviceProvider,
        ILogger<ConversionBackgroundService> logger)
    {
        _taskQueue = taskQueue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Conversion background service starting");
        return ProcessTaskQueueAsync(stoppingToken);
    }

    private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var task = await _taskQueue.DequeueAsync(stoppingToken);
                _logger.LogInformation("Processing conversion task: RecordId={RecordId}, FileName={FileName}",
                    task.RecordId, task.FileName);

                await ProcessConversionTaskAsync(task, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Conversion background service stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred processing task queue");
            }
        }
    }

    private async Task ProcessConversionTaskAsync(ConversionTask task, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var javaClient = scope.ServiceProvider.GetRequiredService<IJavaConverterClient>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ConversionBackgroundService>>();

        var record = await dbContext.ConversionRecords.FindAsync([task.RecordId], stoppingToken);
        if (record == null)
        {
            logger.LogWarning("Conversion record not found: {RecordId}", task.RecordId);
            return;
        }

        try
        {
            record.Status = ConversionStatus.Processing;
            record.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(stoppingToken);

            logger.LogInformation("Calling Java converter for: {RecordId}", task.RecordId);
            var result = await javaClient.ConvertAsync(task.PdfPath, task.OfdPath);

            if (result.Success)
            {
                record.Status = ConversionStatus.Success;
                record.PageCount = result.PageCount;
                logger.LogInformation("Conversion successful: {RecordId}, pages: {Pages}",
                    task.RecordId, result.PageCount);
            }
            else
            {
                record.Status = ConversionStatus.Failed;
                record.ErrorMessage = result.ErrorMessage;
                logger.LogWarning("Conversion failed: {RecordId}, error: {Error}",
                    task.RecordId, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            record.Status = ConversionStatus.Failed;
            record.ErrorMessage = ex.Message;
            logger.LogError(ex, "Conversion exception: {RecordId}", task.RecordId);
        }

        record.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(stoppingToken);
    }
}
