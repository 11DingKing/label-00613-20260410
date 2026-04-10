using PdfToOfd.Api.DTOs;
using PdfToOfd.Api.Models;

namespace PdfToOfd.Api.Services;

public interface IConversionService
{
    Task<UploadResponse> UploadAndConvertAsync(Stream fileStream, string fileName, long fileSize);
    Task<StatusResponse?> GetStatusAsync(long id);
    Task<string?> GetOfdFilePathAsync(long id);
}

public class ConversionService : IConversionService
{
    private readonly ILogger<ConversionService> _logger;
    private readonly Data.AppDbContext _dbContext;
    private readonly IConversionTaskQueue _taskQueue;
    private readonly string _dataPath;

    public ConversionService(
        ILogger<ConversionService> logger,
        Data.AppDbContext dbContext,
        IConversionTaskQueue taskQueue,
        IConfiguration configuration)
    {
        _logger = logger;
        _dbContext = dbContext;
        _taskQueue = taskQueue;
        _dataPath = configuration["DataPath"] ?? "/data";
    }

    public async Task<UploadResponse> UploadAndConvertAsync(Stream fileStream, string fileName, long fileSize)
    {
        var record = new ConversionRecord
        {
            FileName = fileName,
            FileSize = fileSize,
            Status = ConversionStatus.Pending
        };

        _dbContext.ConversionRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created conversion record: {Id} for file: {FileName}", record.Id, fileName);

        var pdfDir = Path.Combine(_dataPath, "pdf");
        var ofdDir = Path.Combine(_dataPath, "ofd");
        Directory.CreateDirectory(pdfDir);
        Directory.CreateDirectory(ofdDir);

        var pdfFileName = $"{record.Id}_{fileName}";
        var pdfPath = Path.Combine(pdfDir, pdfFileName);
        var ofdPath = Path.Combine(ofdDir, $"{record.Id}_{Path.GetFileNameWithoutExtension(fileName)}.ofd");

        record.PdfPath = pdfPath;
        record.OfdPath = ofdPath;
        await _dbContext.SaveChangesAsync();

        await using (var fs = new FileStream(pdfPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fs);
        }

        _logger.LogInformation("Saved PDF file: {Path}", pdfPath);

        var task = new ConversionTask(record.Id, pdfPath, ofdPath, fileName);
        _taskQueue.QueueBackgroundWorkItem(task);

        _logger.LogInformation("Queued conversion task: {Id}", record.Id);

        return new UploadResponse(
            true,
            record.Id,
            "Conversion queued"
        );
    }

    public async Task<StatusResponse?> GetStatusAsync(long id)
    {
        var record = await _dbContext.ConversionRecords.FindAsync(id);
        if (record == null) return null;

        return new StatusResponse(
            record.Id,
            record.FileName,
            record.Status.ToString(),
            record.PageCount,
            record.ErrorMessage,
            record.CreatedAt,
            record.UpdatedAt
        );
    }

    public async Task<string?> GetOfdFilePathAsync(long id)
    {
        var record = await _dbContext.ConversionRecords.FindAsync(id);
        if (record == null || record.Status != ConversionStatus.Success)
            return null;

        return record.OfdPath;
    }
}
