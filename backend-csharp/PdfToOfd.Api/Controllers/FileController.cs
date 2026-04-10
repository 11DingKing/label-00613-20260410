using Microsoft.AspNetCore.Mvc;
using PdfToOfd.Api.DTOs;
using PdfToOfd.Api.Services;

namespace PdfToOfd.Api.Controllers;

[ApiController]
[Route("api/file")]
public class FileController : ControllerBase
{
    private readonly IConversionService _conversionService;
    private readonly ILogger<FileController> _logger;
    private const long MaxFileSize = 100 * 1024 * 1024; // 100MB

    public FileController(IConversionService conversionService, ILogger<FileController> logger)
    {
        _conversionService = conversionService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<ActionResult<UploadResponse>> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Upload attempt with no file");
            return BadRequest(new UploadResponse(false, null, "No file uploaded"));
        }

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Upload attempt with non-PDF file: {FileName}", file.FileName);
            return BadRequest(new UploadResponse(false, null, "Only PDF files are allowed"));
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(new UploadResponse(false, null, "File size exceeds 100MB limit"));
        }

        _logger.LogInformation("Received file upload: {FileName}, size: {Size}", file.FileName, file.Length);

        await using var stream = file.OpenReadStream();
        var result = await _conversionService.UploadAndConvertAsync(stream, file.FileName, file.Length);

        return Ok(result);
    }

    [HttpGet("status/{id:long}")]
    public async Task<ActionResult<StatusResponse>> GetStatus(long id)
    {
        var status = await _conversionService.GetStatusAsync(id);
        if (status == null)
        {
            return NotFound(new { message = "Record not found" });
        }
        return Ok(status);
    }

    [HttpGet("download/{id:long}")]
    public async Task<IActionResult> Download(long id)
    {
        var filePath = await _conversionService.GetOfdFilePathAsync(id);
        if (filePath == null || !System.IO.File.Exists(filePath))
        {
            _logger.LogWarning("Download attempt for non-existent file: {Id}", id);
            return NotFound(new { message = "File not found or conversion not completed" });
        }

        var fileName = Path.GetFileName(filePath);
        _logger.LogInformation("Downloading file: {Id}, path: {Path}", id, filePath);

        return PhysicalFile(filePath, "application/octet-stream", fileName);
    }
}
