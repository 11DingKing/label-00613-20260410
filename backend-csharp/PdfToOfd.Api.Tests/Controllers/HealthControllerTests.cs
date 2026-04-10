using Microsoft.AspNetCore.Mvc;
using Moq;
using PdfToOfd.Api.Controllers;
using PdfToOfd.Api.Services;
using Xunit;

namespace PdfToOfd.Api.Tests.Controllers;

public class HealthControllerTests
{
    private readonly Mock<IJavaConverterClient> _mockJavaClient;
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _mockJavaClient = new Mock<IJavaConverterClient>();
        _controller = new HealthController(_mockJavaClient.Object);
    }

    [Fact]
    public async Task Health_JavaServiceUp_ReturnsAllUp()
    {
        // Arrange
        _mockJavaClient.Setup(c => c.HealthCheckAsync()).ReturnsAsync(true);

        // Act
        var result = await _controller.Health();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        
        var statusProp = response!.GetType().GetProperty("status");
        Assert.Equal("UP", statusProp?.GetValue(response));
    }

    [Fact]
    public async Task Health_JavaServiceDown_ReturnsDependencyDown()
    {
        // Arrange
        _mockJavaClient.Setup(c => c.HealthCheckAsync()).ReturnsAsync(false);

        // Act
        var result = await _controller.Health();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        
        var dependenciesProp = response!.GetType().GetProperty("dependencies");
        var dependencies = dependenciesProp?.GetValue(response);
        var javaConverterProp = dependencies?.GetType().GetProperty("javaConverter");
        Assert.Equal("DOWN", javaConverterProp?.GetValue(dependencies));
    }
}
