using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PdfToOfd.Api.DTOs;
using PdfToOfd.Api.Services;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace PdfToOfd.Api.Tests.Services;

public class JavaConverterClientTests
{
    private readonly Mock<ILogger<JavaConverterClient>> _mockLogger;

    public JavaConverterClientTests()
    {
        _mockLogger = new Mock<ILogger<JavaConverterClient>>();
    }

    private HttpClient CreateMockHttpClient(HttpResponseMessage response)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
    }

    [Fact]
    public async Task ConvertAsync_Success_ReturnsConvertResponse()
    {
        // Arrange
        var expectedResponse = new JavaConvertResponse(true, "/data/ofd/test.ofd", 5, null);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedResponse)
        };
        var httpClient = CreateMockHttpClient(httpResponse);
        var client = new JavaConverterClient(httpClient, _mockLogger.Object);

        // Act
        var result = await client.ConvertAsync("/data/pdf/test.pdf", "/data/ofd/test.ofd");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("/data/ofd/test.ofd", result.OfdPath);
        Assert.Equal(5, result.PageCount);
    }

    [Fact]
    public async Task ConvertAsync_Failure_ReturnsErrorResponse()
    {
        // Arrange
        var expectedResponse = new JavaConvertResponse(false, null, null, "Conversion failed");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedResponse)
        };
        var httpClient = CreateMockHttpClient(httpResponse);
        var client = new JavaConverterClient(httpClient, _mockLogger.Object);

        // Act
        var result = await client.ConvertAsync("/data/pdf/test.pdf", "/data/ofd/test.ofd");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Conversion failed", result.ErrorMessage);
    }

    [Fact]
    public async Task HealthCheckAsync_ServiceUp_ReturnsTrue()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var httpClient = CreateMockHttpClient(httpResponse);
        var client = new JavaConverterClient(httpClient, _mockLogger.Object);

        // Act
        var result = await client.HealthCheckAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HealthCheckAsync_ServiceDown_ReturnsFalse()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        var httpClient = CreateMockHttpClient(httpResponse);
        var client = new JavaConverterClient(httpClient, _mockLogger.Object);

        // Act
        var result = await client.HealthCheckAsync();

        // Assert
        Assert.False(result);
    }
}
