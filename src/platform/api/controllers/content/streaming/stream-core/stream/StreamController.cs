using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Stream;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Streaming.StreamCore.Stream;

[Authorize]
[ApiController]
[Route("api/content/streaming/stream-core/stream")]
[ApiExplorerSettings(GroupName = "content.streaming.stream_core.stream")]
public sealed class StreamController : ControllerBase
{
    private static readonly DomainRoute StreamRoute = new("content", "streaming", "stream");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public StreamController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateStreamRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new CreateStreamCommand(p.StreamId, p.Mode, p.Type, _clock.UtcNow), "stream_created", "content.streaming.stream_core.stream.create_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateStreamRequestModel> request, CancellationToken ct)
        => Dispatch(new ActivateStreamCommand(request.Data.StreamId, _clock.UtcNow), "stream_activated", "content.streaming.stream_core.stream.activate_failed", ct);

    [HttpPost("pause")]
    public Task<IActionResult> Pause([FromBody] ApiRequest<PauseStreamRequestModel> request, CancellationToken ct)
        => Dispatch(new PauseStreamCommand(request.Data.StreamId, _clock.UtcNow), "stream_paused", "content.streaming.stream_core.stream.pause_failed", ct);

    [HttpPost("resume")]
    public Task<IActionResult> Resume([FromBody] ApiRequest<ResumeStreamRequestModel> request, CancellationToken ct)
        => Dispatch(new ResumeStreamCommand(request.Data.StreamId, _clock.UtcNow), "stream_resumed", "content.streaming.stream_core.stream.resume_failed", ct);

    [HttpPost("end")]
    public Task<IActionResult> End([FromBody] ApiRequest<EndStreamRequestModel> request, CancellationToken ct)
        => Dispatch(new EndStreamCommand(request.Data.StreamId, _clock.UtcNow), "stream_ended", "content.streaming.stream_core.stream.end_failed", ct);

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ArchiveStreamRequestModel> request, CancellationToken ct)
        => Dispatch(new ArchiveStreamCommand(request.Data.StreamId, _clock.UtcNow), "stream_archived", "content.streaming.stream_core.stream.archive_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetStream(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_streaming_stream_core_stream.stream_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.streaming.stream_core.stream.not_found", $"Stream {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<StreamReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize StreamReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, StreamRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record CreateStreamRequestModel(Guid StreamId, string Mode, string Type);
public sealed record ActivateStreamRequestModel(Guid StreamId);
public sealed record PauseStreamRequestModel(Guid StreamId);
public sealed record ResumeStreamRequestModel(Guid StreamId);
public sealed record EndStreamRequestModel(Guid StreamId);
public sealed record ArchiveStreamRequestModel(Guid StreamId);
