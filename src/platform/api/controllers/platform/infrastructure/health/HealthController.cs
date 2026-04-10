using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Whyce.Platform.Api.Health;
using Whyce.Shared.Contracts.Infrastructure.Health;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Api.Controllers.Platform.Infrastructure.Health;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(GroupName = "platform.infrastructure.health")]
// phase1.5-S5.2.4 / HC-4 (HEALTH-RATE-LIMITER-EXEMPTION-01): exempt
// every health/liveness/readiness route on this controller from the
// global PC-1 intake limiter so platform/load-balancer probes are
// poll-safe under saturation. Attribute-level exemption keeps the
// fix narrow — Program.cs limiter wiring, partitions, Retry-After,
// and intake metrics are all unchanged. All non-health routes
// continue to flow through the limiter unmodified.
[DisableRateLimiting]
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

        // phase1.5-S5.2.4 / HC-2 (RUNTIME-STATE-AGGREGATION-01):
        // payload now carries the canonical runtimeState + reasons
        // alongside the existing per-IHealthCheck services list. The
        // top-level shape is preserved for backwards compatibility;
        // runtimeState/reasons are additive fields. The HTTP status
        // mapping is the canonical HC-2 mapping driven by
        // RuntimeState (not the legacy Status string).
        var response = new
        {
            status = report.Status,
            runtimeState = report.RuntimeState.ToString(),
            reasons = report.Reasons,
            timestamp = _clock.UtcNow.ToString("o"),
            services = report.Services.Select(s => new
            {
                name = s.Name,
                status = s.IsHealthy ? "HEALTHY" : s.Status,
                latencyMs = s.ResponseTimeMs,
                error = s.Error
            })
        };

        var statusCode = report.RuntimeState switch
        {
            RuntimeState.Healthy => StatusCodes.Status200OK,
            RuntimeState.Degraded => StatusCodes.Status207MultiStatus,
            RuntimeState.NotReady => StatusCodes.Status503ServiceUnavailable,
            RuntimeState.Halt => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status503ServiceUnavailable,
        };

        return StatusCode(statusCode, response);
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "ok" });
    }

    // phase1.5-S5.2.4 / HC-3 (CANONICAL-LIVE-READY-01): canonical
    // liveness endpoint. Process-aliveness only — does NOT consult
    // IRuntimeStateAggregator, dependency health checks, or
    // ApplicationStopping. Returns 200 as long as the process is
    // serving requests. Reserved for platform/load-balancer probes
    // that need pure liveness independent of dependency posture.
    [HttpGet("/live")]
    public IActionResult Live()
    {
        return Ok(new { status = "ok" });
    }

    // phase1.5-S5.2.4 / HC-3 (CANONICAL-LIVE-READY-01): canonical
    // readiness endpoint. Dependency-aware via the HC-2
    // IRuntimeStateAggregator — single source of truth for the
    // runtime-state rule. Healthy/Degraded => 200; NotReady/Halt =>
    // 503. The aggregator already maps ApplicationStopping to
    // NotReady + ["host_draining"], so /ready flips to 503
    // immediately on host drain without any extra state singleton.
    [HttpGet("/ready")]
    public async Task<IActionResult> Ready(
        [FromServices] IRuntimeStateAggregator runtimeStateAggregator,
        CancellationToken cancellationToken)
    {
        var snapshot = await runtimeStateAggregator.GetCurrentStateAsync(cancellationToken);

        var statusCode = snapshot.State switch
        {
            RuntimeState.Healthy => StatusCodes.Status200OK,
            RuntimeState.Degraded => StatusCodes.Status200OK,
            RuntimeState.NotReady => StatusCodes.Status503ServiceUnavailable,
            RuntimeState.Halt => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status503ServiceUnavailable,
        };

        var payload = new
        {
            status = statusCode == StatusCodes.Status200OK ? "ready" : "not_ready",
            runtimeState = snapshot.State.ToString(),
            reasons = snapshot.Reasons,
        };

        return StatusCode(statusCode, payload);
    }
}
