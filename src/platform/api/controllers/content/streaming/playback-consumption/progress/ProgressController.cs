using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Progress;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Streaming.PlaybackConsumption.Progress;

[Authorize]
[ApiController]
[Route("api/content/streaming/playback-consumption/progress")]
[ApiExplorerSettings(GroupName = "content.streaming.playback_consumption.progress")]
public sealed class ProgressController : ControllerBase
{
    private static readonly DomainRoute Route = new("content", "streaming", "progress");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public ProgressController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("track")]
    public Task<IActionResult> Track([FromBody] ApiRequest<TrackProgressRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new TrackProgressCommand(_idGenerator.Generate($"progress:{p.SessionId}"), p.SessionId, p.PositionMs),
            "progress_tracked", "content.streaming.playback_consumption.progress.track_failed", ct);
    }

    [HttpPost("update-position")]
    public Task<IActionResult> UpdatePosition([FromBody] ApiRequest<UpdatePositionRequestModel> request, CancellationToken ct)
        => Dispatch(new UpdatePlaybackPositionCommand(request.Data.ProgressId, request.Data.PositionMs, _clock.UtcNow),
            "playback_position_updated", "content.streaming.playback_consumption.progress.update_failed", ct);

    [HttpPost("pause")]
    public Task<IActionResult> Pause([FromBody] ApiRequest<PausePlaybackRequestModel> request, CancellationToken ct)
        => Dispatch(new PausePlaybackCommand(request.Data.ProgressId, request.Data.PositionMs, _clock.UtcNow),
            "playback_paused", "content.streaming.playback_consumption.progress.pause_failed", ct);

    [HttpPost("resume")]
    public Task<IActionResult> Resume([FromBody] ApiRequest<ProgressIdRequestModel> request, CancellationToken ct)
        => Dispatch(new ResumePlaybackCommand(request.Data.ProgressId, _clock.UtcNow),
            "playback_resumed", "content.streaming.playback_consumption.progress.resume_failed", ct);

    [HttpPost("terminate")]
    public Task<IActionResult> Terminate([FromBody] ApiRequest<ProgressIdRequestModel> request, CancellationToken ct)
        => Dispatch(new TerminateProgressCommand(request.Data.ProgressId, _clock.UtcNow),
            "progress_terminated", "content.streaming.playback_consumption.progress.terminate_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProgress(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_streaming_playback_consumption_progress.progress_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.streaming.playback_consumption.progress.not_found", $"Progress {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<ProgressReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize ProgressReadModel for aggregate {id}.");
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

public sealed record TrackProgressRequestModel(Guid SessionId, long PositionMs);
public sealed record UpdatePositionRequestModel(Guid ProgressId, long PositionMs);
public sealed record PausePlaybackRequestModel(Guid ProgressId, long PositionMs);
public sealed record ProgressIdRequestModel(Guid ProgressId);
