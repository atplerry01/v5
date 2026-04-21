using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Broadcast;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Streaming.LiveStreaming.Broadcast;

[Authorize]
[ApiController]
[Route("api/content/streaming/live-streaming/broadcast")]
[ApiExplorerSettings(GroupName = "content.streaming.live_streaming.broadcast")]
public sealed class BroadcastController : ControllerBase
{
    private static readonly DomainRoute BroadcastRoute = new("content", "streaming", "broadcast");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public BroadcastController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateBroadcastRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new CreateBroadcastCommand(p.BroadcastId, p.StreamId, _clock.UtcNow), "broadcast_created", "content.streaming.live_streaming.broadcast.create_failed", ct);
    }

    [HttpPost("schedule")]
    public Task<IActionResult> Schedule([FromBody] ApiRequest<ScheduleBroadcastRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new ScheduleBroadcastCommand(p.BroadcastId, p.ScheduledStart, p.ScheduledEnd, _clock.UtcNow), "broadcast_scheduled", "content.streaming.live_streaming.broadcast.schedule_failed", ct);
    }

    [HttpPost("start")]
    public Task<IActionResult> Start([FromBody] ApiRequest<StartBroadcastRequestModel> request, CancellationToken ct)
        => Dispatch(new StartBroadcastCommand(request.Data.BroadcastId, _clock.UtcNow), "broadcast_started", "content.streaming.live_streaming.broadcast.start_failed", ct);

    [HttpPost("pause")]
    public Task<IActionResult> Pause([FromBody] ApiRequest<PauseBroadcastRequestModel> request, CancellationToken ct)
        => Dispatch(new PauseBroadcastCommand(request.Data.BroadcastId, _clock.UtcNow), "broadcast_paused", "content.streaming.live_streaming.broadcast.pause_failed", ct);

    [HttpPost("resume")]
    public Task<IActionResult> Resume([FromBody] ApiRequest<ResumeBroadcastRequestModel> request, CancellationToken ct)
        => Dispatch(new ResumeBroadcastCommand(request.Data.BroadcastId, _clock.UtcNow), "broadcast_resumed", "content.streaming.live_streaming.broadcast.resume_failed", ct);

    [HttpPost("end")]
    public Task<IActionResult> End([FromBody] ApiRequest<EndBroadcastRequestModel> request, CancellationToken ct)
        => Dispatch(new EndBroadcastCommand(request.Data.BroadcastId, _clock.UtcNow), "broadcast_ended", "content.streaming.live_streaming.broadcast.end_failed", ct);

    [HttpPost("cancel")]
    public Task<IActionResult> Cancel([FromBody] ApiRequest<CancelBroadcastRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new CancelBroadcastCommand(p.BroadcastId, p.Reason, _clock.UtcNow), "broadcast_cancelled", "content.streaming.live_streaming.broadcast.cancel_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBroadcast(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_streaming_live_streaming_broadcast.broadcast_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.streaming.live_streaming.broadcast.not_found", $"Broadcast {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<BroadcastReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize BroadcastReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, BroadcastRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record CreateBroadcastRequestModel(Guid BroadcastId, Guid StreamId);
public sealed record ScheduleBroadcastRequestModel(Guid BroadcastId, DateTimeOffset ScheduledStart, DateTimeOffset ScheduledEnd);
public sealed record StartBroadcastRequestModel(Guid BroadcastId);
public sealed record PauseBroadcastRequestModel(Guid BroadcastId);
public sealed record ResumeBroadcastRequestModel(Guid BroadcastId);
public sealed record EndBroadcastRequestModel(Guid BroadcastId);
public sealed record CancelBroadcastRequestModel(Guid BroadcastId, string Reason);
