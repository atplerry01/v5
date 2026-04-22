using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Routing.RouteResolution;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Routing.RouteResolution;

[Authorize]
[ApiController]
[Route("api/platform/routing/route-resolution")]
[ApiExplorerSettings(GroupName = "platform.routing.route_resolution")]
public sealed class RouteResolutionController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "routing", "route-resolution");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public RouteResolutionController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _projectionsConnectionString = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("resolve")]
    public Task<IActionResult> Resolve([FromBody] ApiRequest<ResolveRouteRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:routing:route-resolution:{p.SourceDomain}:{p.MessageType}:{_clock.UtcNow.Ticks}");
        var cmd = new ResolveRouteCommand(id, p.SourceClassification, p.SourceContext, p.SourceDomain,
            p.MessageType, p.ResolvedRouteRef, p.ResolutionStrategy, p.DispatchRulesEvaluated, _clock.UtcNow);
        return Dispatch(cmd, "route_resolved", "platform.routing.route_resolution.resolve_failed", ct);
    }

    [HttpPost("fail")]
    public Task<IActionResult> Fail([FromBody] ApiRequest<FailRouteResolutionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:routing:route-resolution:fail:{p.SourceDomain}:{p.MessageType}:{_clock.UtcNow.Ticks}");
        var cmd = new FailRouteResolutionCommand(id, p.SourceClassification, p.SourceContext, p.SourceDomain,
            p.MessageType, p.DispatchRulesEvaluated, p.FailureReason, _clock.UtcNow);
        return Dispatch(cmd, "route_resolution_failed", "platform.routing.route_resolution.fail_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_platform_routing_route_resolution.route_resolution_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("platform.routing.route_resolution.not_found", $"RouteResolution {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<RouteResolutionReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize RouteResolutionReadModel for {id}.");
        return Ok(ApiResponse.Ok(model, RequestCorrelationId(), _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, Route, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }

    private Guid RequestCorrelationId()
    {
        if (HttpContext is { } ctx
            && ctx.Request.Headers.TryGetValue("X-Correlation-Id", out var values)
            && Guid.TryParse(values.ToString(), out var parsed))
            return parsed;
        return Guid.Empty;
    }
}

public sealed record ResolveRouteRequestModel(
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    string MessageType,
    Guid ResolvedRouteRef,
    string ResolutionStrategy,
    IReadOnlyList<Guid> DispatchRulesEvaluated);

public sealed record FailRouteResolutionRequestModel(
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    string MessageType,
    IReadOnlyList<Guid> DispatchRulesEvaluated,
    string FailureReason);
