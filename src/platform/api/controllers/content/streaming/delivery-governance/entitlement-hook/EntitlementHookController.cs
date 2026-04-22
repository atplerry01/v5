using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.EntitlementHook;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Streaming.DeliveryGovernance.EntitlementHook;

[Authorize]
[ApiController]
[Route("api/content/streaming/delivery-governance/entitlement-hook")]
[ApiExplorerSettings(GroupName = "content.streaming.delivery_governance.entitlement_hook")]
public sealed class EntitlementHookController : ControllerBase
{
    private static readonly DomainRoute Route = new("content", "streaming", "entitlement-hook");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public EntitlementHookController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterEntitlementHookRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new RegisterEntitlementHookCommand(_idGenerator.Generate($"entitlement-hook:{p.TargetId}:{p.SourceSystem}"), p.TargetId, p.SourceSystem),
            "entitlement_hook_registered", "content.streaming.delivery_governance.entitlement_hook.register_failed", ct);
    }

    [HttpPost("query")]
    public Task<IActionResult> RecordQuery([FromBody] ApiRequest<RecordEntitlementQueryRequestModel> request, CancellationToken ct)
        => Dispatch(new RecordEntitlementQueryCommand(request.Data.HookId, request.Data.Result, _clock.UtcNow),
            "entitlement_queried", "content.streaming.delivery_governance.entitlement_hook.query_failed", ct);

    [HttpPost("refresh")]
    public Task<IActionResult> Refresh([FromBody] ApiRequest<RefreshEntitlementRequestModel> request, CancellationToken ct)
        => Dispatch(new RefreshEntitlementCommand(request.Data.HookId, request.Data.Result, _clock.UtcNow),
            "entitlement_refreshed", "content.streaming.delivery_governance.entitlement_hook.refresh_failed", ct);

    [HttpPost("invalidate")]
    public Task<IActionResult> Invalidate([FromBody] ApiRequest<InvalidateEntitlementRequestModel> request, CancellationToken ct)
        => Dispatch(new InvalidateEntitlementCommand(request.Data.HookId, _clock.UtcNow),
            "entitlement_invalidated", "content.streaming.delivery_governance.entitlement_hook.invalidate_failed", ct);

    [HttpPost("record-failure")]
    public Task<IActionResult> RecordFailure([FromBody] ApiRequest<RecordEntitlementFailureRequestModel> request, CancellationToken ct)
        => Dispatch(new RecordEntitlementFailureCommand(request.Data.HookId, request.Data.Reason, _clock.UtcNow),
            "entitlement_failure_recorded", "content.streaming.delivery_governance.entitlement_hook.failure_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetHook(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_streaming_delivery_governance_entitlement_hook.entitlement_hook_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.streaming.delivery_governance.entitlement_hook.not_found", $"EntitlementHook {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<EntitlementHookReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize EntitlementHookReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, Route, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record RegisterEntitlementHookRequestModel(Guid TargetId, string SourceSystem);
public sealed record RecordEntitlementQueryRequestModel(Guid HookId, string Result);
public sealed record RefreshEntitlementRequestModel(Guid HookId, string Result);
public sealed record InvalidateEntitlementRequestModel(Guid HookId);
public sealed record RecordEntitlementFailureRequestModel(Guid HookId, string Reason);
