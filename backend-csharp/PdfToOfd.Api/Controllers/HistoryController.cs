using Microsoft.AspNetCore.Mvc;
using PdfToOfd.Api.DTOs;
using PdfToOfd.Api.Services;

namespace PdfToOfd.Api.Controllers;

[ApiController]
[Route("api/history")]
public class HistoryController : ControllerBase
{
    private readonly IHistoryService _historyService;
    private readonly ILogger<HistoryController> _logger;

    public HistoryController(IHistoryService historyService, ILogger<HistoryController> logger)
    {
        _historyService = historyService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<HistoryResponse>> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var result = await _historyService.GetHistoryAsync(page, pageSize);
        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var success = await _historyService.DeleteAsync(id);
        if (!success)
        {
            return NotFound(new { message = "Record not found" });
        }

        _logger.LogInformation("Deleted record: {Id}", id);
        return Ok(new { message = "Deleted successfully" });
    }
}
