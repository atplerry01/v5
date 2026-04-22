using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Routing.DispatchRule;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Routing.DispatchRule;

[Authorize]
[ApiController]
[Route("api/platform/routing/dispatch-rule")]
[ApiExplorerSettings(GroupName = "platform.routing.dispatch_rule")]
public sealed class DispatchRuleController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "routing", "dispatch-rule");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public DispatchRuleController(
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
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterDispatchRuleRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:routing:dispatch-rule:{p.RuleName}:{p.RouteRef}");
        var cmd = new RegisterDispatchRuleCommand(id, p.RuleName, p.RouteRef, p.ConditionType, p.MatchValue, p.Priority, _clock.UtcNow);
        return Dispatch(cmd, "dispatch_rule_registered", "platform.routing.dispatch_rule.register_failed", ct);
    }

    [HttpPost("deactivate")]
    public Task<IActionResult> Deactivate([FromBody] ApiRequest<DeactivateDispatchRuleRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeactivateDispatchRuleCommand(request.Data.DispatchRuleId, _clock.UtcNow);
        return Dispatch(cmd, "dispatch_rule_deactivated", "platform.routing.dispatch_rule.deactivate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_platform_routing_dispatch_rule.dispatch_rule_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("platform.routing.dispatch_rule.not_found", $"DispatchRule {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<DispatchRuleReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize DispatchRuleReadModel for {id}.");
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

public sealed record RegisterDispatchRuleRequestModel(
    string RuleName,
    Guid RouteRef,
    string ConditionType,
    string MatchValue,
    int Priority);

public sealed record DeactivateDispatchRuleRequestModel(Guid DispatchRuleId);
