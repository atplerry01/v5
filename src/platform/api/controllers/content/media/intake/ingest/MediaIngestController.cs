using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Media.Intake.Ingest;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Media.Intake.Ingest;

[Authorize]
[ApiController]
[Route("api/content/media/intake/ingest")]
[ApiExplorerSettings(GroupName = "content.media.intake.ingest")]
public sealed class MediaIngestController : ControllerBase
{
    private static readonly DomainRoute IngestRoute = new("content", "media", "ingest");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public MediaIngestController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("request")]
    public Task<IActionResult> RequestIngest([FromBody] ApiRequest<RequestMediaIngestRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var uploadId = _idGenerator.Generate($"content:media:intake:ingest:{p.SourceRef}:{p.InputRef}");
        return Dispatch(new RequestMediaIngestCommand(uploadId, p.SourceRef, p.InputRef, _clock.UtcNow), "media_ingest_requested", "content.media.intake.ingest.request_failed", ct);
    }

    [HttpPost("accept")]
    public Task<IActionResult> Accept([FromBody] ApiRequest<AcceptMediaIngestRequestModel> request, CancellationToken ct)
        => Dispatch(new AcceptMediaIngestCommand(request.Data.UploadId, _clock.UtcNow), "media_ingest_accepted", "content.media.intake.ingest.accept_failed", ct);

    [HttpPost("start-processing")]
    public Task<IActionResult> StartProcessing([FromBody] ApiRequest<StartMediaIngestProcessingRequestModel> request, CancellationToken ct)
        => Dispatch(new StartMediaIngestProcessingCommand(request.Data.UploadId, _clock.UtcNow), "media_ingest_processing_started", "content.media.intake.ingest.start_processing_failed", ct);

    [HttpPost("complete")]
    public Task<IActionResult> Complete([FromBody] ApiRequest<CompleteMediaIngestRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new CompleteMediaIngestCommand(p.UploadId, p.OutputRef, _clock.UtcNow), "media_ingest_completed", "content.media.intake.ingest.complete_failed", ct);
    }

    [HttpPost("fail")]
    public Task<IActionResult> Fail([FromBody] ApiRequest<FailMediaIngestRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new FailMediaIngestCommand(p.UploadId, p.Reason, _clock.UtcNow), "media_ingest_failed", "content.media.intake.ingest.fail_failed", ct);
    }

    [HttpPost("cancel")]
    public Task<IActionResult> Cancel([FromBody] ApiRequest<CancelMediaIngestRequestModel> request, CancellationToken ct)
        => Dispatch(new CancelMediaIngestCommand(request.Data.UploadId, _clock.UtcNow), "media_ingest_cancelled", "content.media.intake.ingest.cancel_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_media_intake_ingest.media_ingest_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.media.intake.ingest.not_found", $"MediaIngest {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<MediaIngestReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize MediaIngestReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, IngestRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record RequestMediaIngestRequestModel(Guid SourceRef, Guid InputRef);
public sealed record AcceptMediaIngestRequestModel(Guid UploadId);
public sealed record StartMediaIngestProcessingRequestModel(Guid UploadId);
public sealed record CompleteMediaIngestRequestModel(Guid UploadId, Guid OutputRef);
public sealed record FailMediaIngestRequestModel(Guid UploadId, string Reason);
public sealed record CancelMediaIngestRequestModel(Guid UploadId);
