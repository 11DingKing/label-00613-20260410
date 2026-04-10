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
    private readonly IJavaConverterClient _javaClient;
    private readonly string _dataPath;

    public ConversionService(
        ILogger<ConversionService> logger,
        Data.AppDbContext dbContext,
        IJavaConverterClient javaClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _dbContext = dbContext;
        _javaClient = javaClient;
        _dataPath = configuration["DataPath"] ?? "/data";
    }

    public async Task<UploadResponse> UploadAndConvertAsync(Stream fileStream, string fileName, long fileSize)
    {
        // Create record
        var record = new ConversionRecord
        {
            FileName = fileName,
            FileSize = fileSize,
            Status = ConversionStatus.Pending
        };

        _dbContext.ConversionRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created conversion record: {Id} for file: {FileName}", record.Id, fileName);

        // Save PDF file
        var pdfDir = Path.Combine(_dataPath, "pdf");
        var ofdDir = Path.Combine(_dataPath, "ofd");
        Directory.CreateDirectory(pdfDir);
        Directory.CreateDirectory(ofdDir);

        var pdfFileName = $"{record.Id}_{fileName}";
        var pdfPath = Path.Combine(pdfDir, pdfFileName);
        var ofdPath = Path.Combine(ofdDir, $"{record.Id}_{Path.GetFileNameWithoutExtension(fileName)}.ofd");

        record.PdfPath = pdfPath;
        record.OfdPath = ofdPath;
        record.Status = ConversionStatus.Processing;
        await _dbContext.SaveChangesAsync();

        // Save file
        await using (var fs = new FileStream(pdfPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fs);
        }

        _logger.LogInformation("Saved PDF file: {Path}", pdfPath);

        // Call Java converter
        try
        {
            var result = await _javaClient.ConvertAsync(pdfPath, ofdPath);

            if (result.Success)
            {
                record.Status = ConversionStatus.Success;
                record.PageCount = result.PageCount;
                _logger.LogInformation("Conversion successful: {Id}, pages: {Pages}", record.Id, result.PageCount);
            }
            else
            {
                record.Status = ConversionStatus.Failed;
                record.ErrorMessage = result.ErrorMessage;
                _logger.LogWarning("Conversion failed: {Id}, error: {Error}", record.Id, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            record.Status = ConversionStatus.Failed;
            record.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Conversion exception: {Id}", record.Id);
        }

        record.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return new UploadResponse(
            record.Status == ConversionStatus.Success,
            record.Id,
            record.Status == ConversionStatus.Success ? "Conversion completed" : record.ErrorMessage
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
