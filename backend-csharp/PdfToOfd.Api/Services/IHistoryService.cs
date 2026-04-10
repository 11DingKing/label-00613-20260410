using Microsoft.EntityFrameworkCore;
using PdfToOfd.Api.Data;
using PdfToOfd.Api.DTOs;

namespace PdfToOfd.Api.Services;

public interface IHistoryService
{
    Task<HistoryResponse> GetHistoryAsync(int page, int pageSize);
    Task<bool> DeleteAsync(long id);
}

public class HistoryService : IHistoryService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<HistoryService> _logger;

    public HistoryService(AppDbContext dbContext, ILogger<HistoryService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<HistoryResponse> GetHistoryAsync(int page, int pageSize)
    {
        var query = _dbContext.ConversionRecords
            .OrderByDescending(r => r.CreatedAt);

        var total = await query.CountAsync();
        var records = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new StatusResponse(
                r.Id,
                r.FileName,
                r.Status.ToString(),
                r.PageCount,
                r.ErrorMessage,
                r.CreatedAt,
                r.UpdatedAt
            ))
            .ToListAsync();

        return new HistoryResponse(records, total, page, pageSize);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var record = await _dbContext.ConversionRecords.FindAsync(id);
        if (record == null) return false;

        // Delete files
        try
        {
            if (!string.IsNullOrEmpty(record.PdfPath) && File.Exists(record.PdfPath))
                File.Delete(record.PdfPath);
            if (!string.IsNullOrEmpty(record.OfdPath) && File.Exists(record.OfdPath))
                File.Delete(record.OfdPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete files for record {Id}", id);
        }

        _dbContext.ConversionRecords.Remove(record);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted conversion record: {Id}", id);
        return true;
    }
}
