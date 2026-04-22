using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Routing.RouteDefinition;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Routing.RouteDefinition;

[Authorize]
[ApiController]
[Route("api/platform/routing/route-definition")]
[ApiExplorerSettings(GroupName = "platform.routing.route_definition")]
public sealed class RouteDefinitionController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "routing", "route-definition");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public RouteDefinitionController(
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

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterRouteDefinitionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:routing:route-definition:{p.RouteName}:{p.SourceDomain}:{p.DestinationDomain}");
        var cmd = new RegisterRouteDefinitionCommand(id, p.RouteName,
            p.SourceClassification, p.SourceContext, p.SourceDomain,
            p.DestinationClassification, p.DestinationContext, p.DestinationDomain,
            p.TransportHint, p.Priority, _clock.UtcNow);
        return Dispatch(cmd, "route_definition_registered", "platform.routing.route_definition.register_failed", ct);
    }

    [HttpPost("deactivate")]
    public Task<IActionResult> Deactivate([FromBody] ApiRequest<DeactivateRouteDefinitionRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeactivateRouteDefinitionCommand(request.Data.RouteDefinitionId, _clock.UtcNow);
        return Dispatch(cmd, "route_definition_deactivated", "platform.routing.route_definition.deactivate_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<DeprecateRouteDefinitionRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecateRouteDefinitionCommand(request.Data.RouteDefinitionId, _clock.UtcNow);
        return Dispatch(cmd, "route_definition_deprecated", "platform.routing.route_definition.deprecate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_platform_routing_route_definition.route_definition_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("platform.routing.route_definition.not_found", $"RouteDefinition {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<RouteDefinitionReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize RouteDefinitionReadModel for {id}.");
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

public sealed record RegisterRouteDefinitionRequestModel(
    string RouteName,
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    string DestinationClassification,
    string DestinationContext,
    string DestinationDomain,
    string TransportHint,
    int Priority);

public sealed record DeactivateRouteDefinitionRequestModel(Guid RouteDefinitionId);
public sealed record DeprecateRouteDefinitionRequestModel(Guid RouteDefinitionId);
