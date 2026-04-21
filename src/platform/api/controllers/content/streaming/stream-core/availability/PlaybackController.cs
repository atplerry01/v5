using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Availability;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Streaming.StreamCore.Availability;

[Authorize]
[ApiController]
[Route("api/content/streaming/stream-core/availability")]
[ApiExplorerSettings(GroupName = "content.streaming.stream_core.availability")]
public sealed class PlaybackController : ControllerBase
{
    private static readonly DomainRoute PlaybackRoute = new("content", "streaming", "availability");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public PlaybackController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreatePlaybackRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(
            new CreatePlaybackCommand(p.PlaybackId, p.SourceId, p.SourceKind, p.Mode, p.AvailableFrom, p.AvailableUntil, _clock.UtcNow),
            "playback_created",
            "content.streaming.stream_core.availability.create_failed",
            ct);
    }

    [HttpPost("enable")]
    public Task<IActionResult> Enable([FromBody] ApiRequest<EnablePlaybackRequestModel> request, CancellationToken ct)
        => Dispatch(new EnablePlaybackCommand(request.Data.PlaybackId, _clock.UtcNow), "playback_enabled", "content.streaming.stream_core.availability.enable_failed", ct);

    [HttpPost("disable")]
    public Task<IActionResult> Disable([FromBody] ApiRequest<DisablePlaybackRequestModel> request, CancellationToken ct)
        => Dispatch(new DisablePlaybackCommand(request.Data.PlaybackId, request.Data.Reason, _clock.UtcNow), "playback_disabled", "content.streaming.stream_core.availability.disable_failed", ct);

    [HttpPost("update-window")]
    public Task<IActionResult> UpdateWindow([FromBody] ApiRequest<UpdatePlaybackWindowRequestModel> request, CancellationToken ct)
        => Dispatch(
            new UpdatePlaybackWindowCommand(request.Data.PlaybackId, request.Data.AvailableFrom, request.Data.AvailableUntil, _clock.UtcNow),
            "playback_window_updated",
            "content.streaming.stream_core.availability.update_window_failed",
            ct);

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ArchivePlaybackRequestModel> request, CancellationToken ct)
        => Dispatch(new ArchivePlaybackCommand(request.Data.PlaybackId, _clock.UtcNow), "playback_archived", "content.streaming.stream_core.availability.archive_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPlayback(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_streaming_stream_core_availability.availability_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.streaming.stream_core.availability.not_found", $"Playback {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<PlaybackReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize PlaybackReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, PlaybackRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record CreatePlaybackRequestModel(Guid PlaybackId, Guid SourceId, string SourceKind, string Mode, DateTimeOffset AvailableFrom, DateTimeOffset AvailableUntil);
public sealed record EnablePlaybackRequestModel(Guid PlaybackId);
public sealed record DisablePlaybackRequestModel(Guid PlaybackId, string Reason);
public sealed record UpdatePlaybackWindowRequestModel(Guid PlaybackId, DateTimeOffset AvailableFrom, DateTimeOffset AvailableUntil);
public sealed record ArchivePlaybackRequestModel(Guid PlaybackId);
