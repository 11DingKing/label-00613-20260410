using Microsoft.AspNetCore.Mvc;
using PdfToOfd.Api.Services;

namespace PdfToOfd.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly IJavaConverterClient _javaClient;

    public HealthController(IJavaConverterClient javaClient)
    {
        _javaClient = javaClient;
    }

    [HttpGet]
    public async Task<IActionResult> Health()
    {
        var javaHealthy = await _javaClient.HealthCheckAsync();

        return Ok(new
        {
            status = "UP",
            service = "pdf-to-ofd-api",
            version = "1.0.0",
            dependencies = new
            {
                javaConverter = javaHealthy ? "UP" : "DOWN"
            }
        });
    }
}
