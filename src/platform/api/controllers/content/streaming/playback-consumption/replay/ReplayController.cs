using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Replay;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Streaming.PlaybackConsumption.Replay;

[Authorize]
[ApiController]
[Route("api/content/streaming/playback-consumption/replay")]
[ApiExplorerSettings(GroupName = "content.streaming.playback_consumption.replay")]
public sealed class ReplayController : ControllerBase
{
    private static readonly DomainRoute Route = new("content", "streaming", "replay");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public ReplayController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("request")]
    public Task<IActionResult> Request([FromBody] ApiRequest<RequestReplayRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new RequestReplayCommand(_idGenerator.Generate($"replay:{p.ArchiveId}:{p.ViewerId}"), p.ArchiveId, p.ViewerId),
            "replay_requested", "content.streaming.playback_consumption.replay.request_failed", ct);
    }

    [HttpPost("start")]
    public Task<IActionResult> Start([FromBody] ApiRequest<StartReplayRequestModel> request, CancellationToken ct)
        => Dispatch(new StartReplayCommand(request.Data.ReplayId, request.Data.PositionMs, _clock.UtcNow),
            "replay_started", "content.streaming.playback_consumption.replay.start_failed", ct);

    [HttpPost("pause")]
    public Task<IActionResult> Pause([FromBody] ApiRequest<PauseReplayRequestModel> request, CancellationToken ct)
        => Dispatch(new PauseReplayCommand(request.Data.ReplayId, request.Data.PositionMs, _clock.UtcNow),
            "replay_paused", "content.streaming.playback_consumption.replay.pause_failed", ct);

    [HttpPost("resume")]
    public Task<IActionResult> Resume([FromBody] ApiRequest<ReplayIdRequestModel> request, CancellationToken ct)
        => Dispatch(new ResumeReplayCommand(request.Data.ReplayId, _clock.UtcNow),
            "replay_resumed", "content.streaming.playback_consumption.replay.resume_failed", ct);

    [HttpPost("complete")]
    public Task<IActionResult> Complete([FromBody] ApiRequest<CompleteReplayRequestModel> request, CancellationToken ct)
        => Dispatch(new CompleteReplayCommand(request.Data.ReplayId, request.Data.PositionMs, _clock.UtcNow),
            "replay_completed", "content.streaming.playback_consumption.replay.complete_failed", ct);

    [HttpPost("abandon")]
    public Task<IActionResult> Abandon([FromBody] ApiRequest<ReplayIdRequestModel> request, CancellationToken ct)
        => Dispatch(new AbandonReplayCommand(request.Data.ReplayId, _clock.UtcNow),
            "replay_abandoned", "content.streaming.playback_consumption.replay.abandon_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReplay(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_streaming_playback_consumption_replay.replay_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.streaming.playback_consumption.replay.not_found", $"Replay {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<ReplayReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize ReplayReadModel for aggregate {id}.");
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

public sealed record RequestReplayRequestModel(Guid ArchiveId, Guid ViewerId);
public sealed record StartReplayRequestModel(Guid ReplayId, long PositionMs);
public sealed record PauseReplayRequestModel(Guid ReplayId, long PositionMs);
public sealed record ReplayIdRequestModel(Guid ReplayId);
public sealed record CompleteReplayRequestModel(Guid ReplayId, long PositionMs);
