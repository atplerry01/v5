using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.IngestSession;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Streaming.LiveStreaming.IngestSession;

[Authorize]
[ApiController]
[Route("api/content/streaming/live-streaming/ingest-session")]
[ApiExplorerSettings(GroupName = "content.streaming.live_streaming.ingest_session")]
public sealed class IngestSessionController : ControllerBase
{
    private static readonly DomainRoute Route = new("content", "streaming", "ingest-session");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public IngestSessionController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("authenticate")]
    public Task<IActionResult> Authenticate([FromBody] ApiRequest<AuthenticateIngestSessionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new AuthenticateIngestSessionCommand(_idGenerator.Generate($"ingest-session:{p.BroadcastId}:{p.Endpoint}"), p.BroadcastId, p.Endpoint),
            "ingest_session_authenticated", "content.streaming.live_streaming.ingest_session.authenticate_failed", ct);
    }

    [HttpPost("start-streaming")]
    public Task<IActionResult> StartStreaming([FromBody] ApiRequest<IngestSessionIdRequestModel> request, CancellationToken ct)
        => Dispatch(new StartIngestStreamingCommand(request.Data.SessionId, _clock.UtcNow),
            "ingest_streaming_started", "content.streaming.live_streaming.ingest_session.start_failed", ct);

    [HttpPost("stall")]
    public Task<IActionResult> Stall([FromBody] ApiRequest<IngestSessionIdRequestModel> request, CancellationToken ct)
        => Dispatch(new StallIngestSessionCommand(request.Data.SessionId, _clock.UtcNow),
            "ingest_session_stalled", "content.streaming.live_streaming.ingest_session.stall_failed", ct);

    [HttpPost("resume")]
    public Task<IActionResult> Resume([FromBody] ApiRequest<IngestSessionIdRequestModel> request, CancellationToken ct)
        => Dispatch(new ResumeIngestSessionCommand(request.Data.SessionId, _clock.UtcNow),
            "ingest_session_resumed", "content.streaming.live_streaming.ingest_session.resume_failed", ct);

    [HttpPost("end")]
    public Task<IActionResult> End([FromBody] ApiRequest<IngestSessionIdRequestModel> request, CancellationToken ct)
        => Dispatch(new EndIngestSessionCommand(request.Data.SessionId, _clock.UtcNow),
            "ingest_session_ended", "content.streaming.live_streaming.ingest_session.end_failed", ct);

    [HttpPost("fail")]
    public Task<IActionResult> Fail([FromBody] ApiRequest<FailIngestSessionRequestModel> request, CancellationToken ct)
        => Dispatch(new FailIngestSessionCommand(request.Data.SessionId, request.Data.FailureReason, _clock.UtcNow),
            "ingest_session_failed", "content.streaming.live_streaming.ingest_session.fail_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSession(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_streaming_live_streaming_ingest_session.ingest_session_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.streaming.live_streaming.ingest_session.not_found", $"IngestSession {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<IngestSessionReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize IngestSessionReadModel for aggregate {id}.");
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

public sealed record AuthenticateIngestSessionRequestModel(Guid BroadcastId, string Endpoint);
public sealed record IngestSessionIdRequestModel(Guid SessionId);
public sealed record FailIngestSessionRequestModel(Guid SessionId, string FailureReason);
