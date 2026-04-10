using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PdfToOfd.Api.Controllers;
using PdfToOfd.Api.DTOs;
using PdfToOfd.Api.Services;
using Xunit;

namespace PdfToOfd.Api.Tests.Controllers;

public class FileControllerTests
{
    private readonly Mock<IConversionService> _mockConversionService;
    private readonly Mock<ILogger<FileController>> _mockLogger;
    private readonly FileController _controller;

    public FileControllerTests()
    {
        _mockConversionService = new Mock<IConversionService>();
        _mockLogger = new Mock<ILogger<FileController>>();
        _controller = new FileController(_mockConversionService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Upload_NoFile_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Upload(null!);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<UploadResponse>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Equal("No file uploaded", response.Message);
    }

    [Fact]
    public async Task Upload_EmptyFile_ReturnsBadRequest()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0);

        // Act
        var result = await _controller.Upload(mockFile.Object);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<UploadResponse>(badRequest.Value);
        Assert.False(response.Success);
    }


    [Fact]
    public async Task Upload_NonPdfFile_ReturnsBadRequest()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.FileName).Returns("test.txt");

        // Act
        var result = await _controller.Upload(mockFile.Object);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<UploadResponse>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Equal("Only PDF files are allowed", response.Message);
    }

    [Fact]
    public async Task Upload_ValidPdf_ReturnsOk()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var content = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF magic bytes
        var stream = new MemoryStream(content);
        
        mockFile.Setup(f => f.Length).Returns(content.Length);
        mockFile.Setup(f => f.FileName).Returns("test.pdf");
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

        var expectedResponse = new UploadResponse(true, 1, "Conversion queued");
        _mockConversionService
            .Setup(s => s.UploadAndConvertAsync(It.IsAny<Stream>(), "test.pdf", content.Length))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Upload(mockFile.Object);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UploadResponse>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(1, response.RecordId);
    }

    [Fact]
    public async Task GetStatus_RecordExists_ReturnsOk()
    {
        // Arrange
        var statusResponse = new StatusResponse(1, "test.pdf", "Success", 5, null, DateTime.UtcNow, DateTime.UtcNow);
        _mockConversionService.Setup(s => s.GetStatusAsync(1)).ReturnsAsync(statusResponse);

        // Act
        var result = await _controller.GetStatus(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<StatusResponse>(okResult.Value);
        Assert.Equal("Success", response.Status);
    }

    [Fact]
    public async Task GetStatus_RecordNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockConversionService.Setup(s => s.GetStatusAsync(999)).ReturnsAsync((StatusResponse?)null);

        // Act
        var result = await _controller.GetStatus(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Download_FileNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockConversionService.Setup(s => s.GetOfdFilePathAsync(1)).ReturnsAsync((string?)null);

        // Act
        var result = await _controller.Download(1);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
