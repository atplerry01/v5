using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Archive;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Streaming.LiveStreaming.Archive;

[Authorize]
[ApiController]
[Route("api/content/streaming/live-streaming/archive")]
[ApiExplorerSettings(GroupName = "content.streaming.live_streaming.archive")]
public sealed class ArchiveController : ControllerBase
{
    private static readonly DomainRoute ArchiveRoute = new("content", "streaming", "archive");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public ArchiveController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("start")]
    public Task<IActionResult> Start([FromBody] ApiRequest<StartArchiveRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new StartArchiveCommand(p.ArchiveId, p.StreamId, p.SessionId, _clock.UtcNow), "archive_started", "content.streaming.live_streaming.archive.start_failed", ct);
    }

    [HttpPost("complete")]
    public Task<IActionResult> Complete([FromBody] ApiRequest<CompleteArchiveRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new CompleteArchiveCommand(p.ArchiveId, p.OutputId, _clock.UtcNow), "archive_completed", "content.streaming.live_streaming.archive.complete_failed", ct);
    }

    [HttpPost("fail")]
    public Task<IActionResult> Fail([FromBody] ApiRequest<FailArchiveRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new FailArchiveCommand(p.ArchiveId, p.Reason, _clock.UtcNow), "archive_failed", "content.streaming.live_streaming.archive.fail_failed", ct);
    }

    [HttpPost("finalize")]
    public Task<IActionResult> Finalize([FromBody] ApiRequest<FinalizeArchiveRequestModel> request, CancellationToken ct)
        => Dispatch(new FinalizeArchiveCommand(request.Data.ArchiveId, _clock.UtcNow), "archive_finalized", "content.streaming.live_streaming.archive.finalize_failed", ct);

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ArchiveArchiveRequestModel> request, CancellationToken ct)
        => Dispatch(new ArchiveArchiveCommand(request.Data.ArchiveId, _clock.UtcNow), "archive_archived", "content.streaming.live_streaming.archive.archive_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetArchive(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_streaming_live_streaming_archive.archive_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.streaming.live_streaming.archive.not_found", $"Archive {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<ArchiveReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize ArchiveReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, ArchiveRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record StartArchiveRequestModel(Guid ArchiveId, Guid StreamId, Guid? SessionId);
public sealed record CompleteArchiveRequestModel(Guid ArchiveId, Guid OutputId);
public sealed record FailArchiveRequestModel(Guid ArchiveId, string Reason);
public sealed record FinalizeArchiveRequestModel(Guid ArchiveId);
public sealed record ArchiveArchiveRequestModel(Guid ArchiveId);
