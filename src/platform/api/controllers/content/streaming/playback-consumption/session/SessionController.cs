using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Session;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Streaming.PlaybackConsumption.Session;

[Authorize]
[ApiController]
[Route("api/content/streaming/playback-consumption/session")]
[ApiExplorerSettings(GroupName = "content.streaming.playback_consumption.session")]
public sealed class SessionController : ControllerBase
{
    private static readonly DomainRoute SessionRoute = new("content", "streaming", "session");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public SessionController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("open")]
    public Task<IActionResult> Open([FromBody] ApiRequest<OpenSessionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new OpenSessionCommand(p.SessionId, p.StreamId, _clock.UtcNow, p.ExpiresAt), "session_opened", "content.streaming.playback_consumption.session.open_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateSessionRequestModel> request, CancellationToken ct)
        => Dispatch(new ActivateSessionCommand(request.Data.SessionId, _clock.UtcNow), "session_activated", "content.streaming.playback_consumption.session.activate_failed", ct);

    [HttpPost("suspend")]
    public Task<IActionResult> Suspend([FromBody] ApiRequest<SuspendSessionRequestModel> request, CancellationToken ct)
        => Dispatch(new SuspendSessionCommand(request.Data.SessionId, _clock.UtcNow), "session_suspended", "content.streaming.playback_consumption.session.suspend_failed", ct);

    [HttpPost("resume")]
    public Task<IActionResult> Resume([FromBody] ApiRequest<ResumeSessionRequestModel> request, CancellationToken ct)
        => Dispatch(new ResumeSessionCommand(request.Data.SessionId, _clock.UtcNow), "session_resumed", "content.streaming.playback_consumption.session.resume_failed", ct);

    [HttpPost("close")]
    public Task<IActionResult> Close([FromBody] ApiRequest<CloseSessionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new CloseSessionCommand(p.SessionId, p.Reason, _clock.UtcNow), "session_closed", "content.streaming.playback_consumption.session.close_failed", ct);
    }

    [HttpPost("fail")]
    public Task<IActionResult> Fail([FromBody] ApiRequest<FailSessionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new FailSessionCommand(p.SessionId, p.Reason, _clock.UtcNow), "session_failed", "content.streaming.playback_consumption.session.fail_failed", ct);
    }

    [HttpPost("expire")]
    public Task<IActionResult> Expire([FromBody] ApiRequest<ExpireSessionRequestModel> request, CancellationToken ct)
        => Dispatch(new ExpireSessionCommand(request.Data.SessionId, _clock.UtcNow), "session_expired", "content.streaming.playback_consumption.session.expire_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSession(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_streaming_playback_consumption_session.session_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.streaming.playback_consumption.session.not_found", $"Session {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<SessionReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize SessionReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, SessionRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record OpenSessionRequestModel(Guid SessionId, Guid StreamId, DateTimeOffset ExpiresAt);
public sealed record ActivateSessionRequestModel(Guid SessionId);
public sealed record SuspendSessionRequestModel(Guid SessionId);
public sealed record ResumeSessionRequestModel(Guid SessionId);
public sealed record CloseSessionRequestModel(Guid SessionId, string Reason);
public sealed record FailSessionRequestModel(Guid SessionId, string Reason);
public sealed record ExpireSessionRequestModel(Guid SessionId);
