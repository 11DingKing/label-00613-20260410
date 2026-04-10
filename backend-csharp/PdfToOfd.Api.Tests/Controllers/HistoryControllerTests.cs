using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PdfToOfd.Api.Controllers;
using PdfToOfd.Api.DTOs;
using PdfToOfd.Api.Services;
using Xunit;

namespace PdfToOfd.Api.Tests.Controllers;

public class HistoryControllerTests
{
    private readonly Mock<IHistoryService> _mockHistoryService;
    private readonly Mock<ILogger<HistoryController>> _mockLogger;
    private readonly HistoryController _controller;

    public HistoryControllerTests()
    {
        _mockHistoryService = new Mock<IHistoryService>();
        _mockLogger = new Mock<ILogger<HistoryController>>();
        _controller = new HistoryController(_mockHistoryService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetHistory_DefaultParams_ReturnsOk()
    {
        // Arrange
        var records = new List<StatusResponse>
        {
            new(1, "test1.pdf", "Success", 5, null, DateTime.UtcNow, DateTime.UtcNow),
            new(2, "test2.pdf", "Failed", null, "Error", DateTime.UtcNow, DateTime.UtcNow)
        };
        var historyResponse = new HistoryResponse(records, 2, 1, 10);
        _mockHistoryService.Setup(s => s.GetHistoryAsync(1, 10)).ReturnsAsync(historyResponse);

        // Act
        var result = await _controller.GetHistory();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<HistoryResponse>(okResult.Value);
        Assert.Equal(2, response.Total);
        Assert.Equal(2, response.Records.Count);
    }

    [Fact]
    public async Task GetHistory_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var records = new List<StatusResponse>
        {
            new(3, "test3.pdf", "Success", 3, null, DateTime.UtcNow, DateTime.UtcNow)
        };
        var historyResponse = new HistoryResponse(records, 10, 2, 5);
        _mockHistoryService.Setup(s => s.GetHistoryAsync(2, 5)).ReturnsAsync(historyResponse);

        // Act
        var result = await _controller.GetHistory(page: 2, pageSize: 5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<HistoryResponse>(okResult.Value);
        Assert.Equal(2, response.Page);
        Assert.Equal(5, response.PageSize);
    }

    [Fact]
    public async Task GetHistory_InvalidPage_NormalizesToOne()
    {
        // Arrange
        var historyResponse = new HistoryResponse(new List<StatusResponse>(), 0, 1, 10);
        _mockHistoryService.Setup(s => s.GetHistoryAsync(1, 10)).ReturnsAsync(historyResponse);

        // Act
        var result = await _controller.GetHistory(page: -5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        _mockHistoryService.Verify(s => s.GetHistoryAsync(1, 10), Times.Once);
    }

    [Fact]
    public async Task Delete_RecordExists_ReturnsOk()
    {
        // Arrange
        _mockHistoryService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Delete_RecordNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockHistoryService.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
