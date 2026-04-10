using PdfToOfd.Api.DTOs;

namespace PdfToOfd.Api.Services;

public interface IJavaConverterClient
{
    Task<JavaConvertResponse> ConvertAsync(string pdfPath, string ofdPath);
    Task<bool> HealthCheckAsync();
}

public class JavaConverterClient : IJavaConverterClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JavaConverterClient> _logger;

    public JavaConverterClient(HttpClient httpClient, ILogger<JavaConverterClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<JavaConvertResponse> ConvertAsync(string pdfPath, string ofdPath)
    {
        var request = new JavaConvertRequest(pdfPath, ofdPath);
        _logger.LogInformation("Calling Java converter: {PdfPath} -> {OfdPath}", pdfPath, ofdPath);

        var response = await _httpClient.PostAsJsonAsync("/api/convert", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JavaConvertResponse>();
        return result ?? new JavaConvertResponse(false, null, null, "Empty response from converter");
    }

    public async Task<bool> HealthCheckAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
