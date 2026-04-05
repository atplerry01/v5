using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Whyce.Platform.Host.Health;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Host.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly HealthAggregator _healthAggregator;
    private readonly IClock _clock;

    public HealthController(HealthAggregator healthAggregator, IClock clock)
    {
        _healthAggregator = healthAggregator;
        _clock = clock;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        var report = await _healthAggregator.CheckAllAsync();

        var response = new
        {
            status = report.Status,
            timestamp = _clock.UtcNow.ToString("o"),
            services = report.Services.Select(s => new
            {
                name = s.Name,
                status = s.IsHealthy ? "HEALTHY" : s.Status,
                latencyMs = s.ResponseTimeMs,
                error = s.Error
            })
        };

        var statusCode = report.Status switch
        {
            "HEALTHY" => StatusCodes.Status200OK,
            "DEGRADED" => StatusCodes.Status207MultiStatus,
            _ => StatusCodes.Status503ServiceUnavailable
        };

        return StatusCode(statusCode, response);
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "ok" });
    }
}
