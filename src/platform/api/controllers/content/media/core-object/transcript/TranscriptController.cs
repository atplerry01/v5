using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Transcript;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Media.CoreObject.Transcript;

[Authorize]
[ApiController]
[Route("api/content/media/core-object/transcript")]
[ApiExplorerSettings(GroupName = "content.media.core_object.transcript")]
public sealed class TranscriptController : ControllerBase
{
    private static readonly DomainRoute TranscriptRoute = new("content", "media", "transcript");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public TranscriptController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateTranscriptRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var transcriptId = _idGenerator.Generate($"content:media:core-object:transcript:{p.AssetRef}:{p.Language}:{p.Format}");
        return Dispatch(new CreateTranscriptCommand(transcriptId, p.AssetRef, p.SourceFileRef, p.Format, p.Language, _clock.UtcNow),
            "transcript_created", "content.media.core_object.transcript.create_failed", ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateTranscriptRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new UpdateTranscriptCommand(p.TranscriptId, p.OutputRef, _clock.UtcNow), "transcript_updated", "content.media.core_object.transcript.update_failed", ct);
    }

    [HttpPost("finalize")]
    public Task<IActionResult> Finalize([FromBody] ApiRequest<FinalizeTranscriptRequestModel> request, CancellationToken ct)
        => Dispatch(new FinalizeTranscriptCommand(request.Data.TranscriptId, _clock.UtcNow), "transcript_finalized", "content.media.core_object.transcript.finalize_failed", ct);

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ArchiveTranscriptRequestModel> request, CancellationToken ct)
        => Dispatch(new ArchiveTranscriptCommand(request.Data.TranscriptId, _clock.UtcNow), "transcript_archived", "content.media.core_object.transcript.archive_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_media_core_object_transcript.transcript_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.media.core_object.transcript.not_found", $"Transcript {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<TranscriptReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize TranscriptReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, TranscriptRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record CreateTranscriptRequestModel(Guid AssetRef, Guid? SourceFileRef, string Format, string Language);
public sealed record UpdateTranscriptRequestModel(Guid TranscriptId, Guid OutputRef);
public sealed record FinalizeTranscriptRequestModel(Guid TranscriptId);
public sealed record ArchiveTranscriptRequestModel(Guid TranscriptId);
