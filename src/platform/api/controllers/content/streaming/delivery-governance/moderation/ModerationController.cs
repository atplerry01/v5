using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Moderation;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Streaming.DeliveryGovernance.Moderation;

[Authorize]
[ApiController]
[Route("api/content/streaming/delivery-governance/moderation")]
[ApiExplorerSettings(GroupName = "content.streaming.delivery_governance.moderation")]
public sealed class ModerationController : ControllerBase
{
    private static readonly DomainRoute Route = new("content", "streaming", "moderation");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public ModerationController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("flag")]
    public Task<IActionResult> Flag([FromBody] ApiRequest<FlagStreamRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new FlagStreamCommand(_idGenerator.Generate($"moderation:{p.TargetId}:{p.FlagReason}"), p.TargetId, p.FlagReason),
            "stream_flagged", "content.streaming.delivery_governance.moderation.flag_failed", ct);
    }

    [HttpPost("assign")]
    public Task<IActionResult> Assign([FromBody] ApiRequest<AssignModerationRequestModel> request, CancellationToken ct)
        => Dispatch(new AssignModerationCommand(request.Data.ModerationId, request.Data.ModeratorId, _clock.UtcNow),
            "moderation_assigned", "content.streaming.delivery_governance.moderation.assign_failed", ct);

    [HttpPost("decide")]
    public Task<IActionResult> Decide([FromBody] ApiRequest<DecideModerationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new DecideModerationCommand(p.ModerationId, p.Decision, p.Rationale, _clock.UtcNow),
            "moderation_decided", "content.streaming.delivery_governance.moderation.decide_failed", ct);
    }

    [HttpPost("overturn")]
    public Task<IActionResult> Overturn([FromBody] ApiRequest<OverturnModerationRequestModel> request, CancellationToken ct)
        => Dispatch(new OverturnModerationCommand(request.Data.ModerationId, request.Data.Rationale, _clock.UtcNow),
            "moderation_overturned", "content.streaming.delivery_governance.moderation.overturn_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetModeration(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_streaming_delivery_governance_moderation.moderation_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.streaming.delivery_governance.moderation.not_found", $"Moderation {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<ModerationReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize ModerationReadModel for aggregate {id}.");
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

public sealed record FlagStreamRequestModel(Guid TargetId, string FlagReason);
public sealed record AssignModerationRequestModel(Guid ModerationId, Guid ModeratorId);
public sealed record DecideModerationRequestModel(Guid ModerationId, string Decision, string Rationale);
public sealed record OverturnModerationRequestModel(Guid ModerationId, string Rationale);
