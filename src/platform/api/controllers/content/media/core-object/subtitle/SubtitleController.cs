using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Subtitle;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Media.CoreObject.Subtitle;

[Authorize]
[ApiController]
[Route("api/content/media/core-object/subtitle")]
[ApiExplorerSettings(GroupName = "content.media.core_object.subtitle")]
public sealed class SubtitleController : ControllerBase
{
    private static readonly DomainRoute SubtitleRoute = new("content", "media", "subtitle");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public SubtitleController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateSubtitleRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var subtitleId = _idGenerator.Generate($"content:media:core-object:subtitle:{p.AssetRef}:{p.Language}:{p.Format}");
        return Dispatch(new CreateSubtitleCommand(subtitleId, p.AssetRef, p.SourceFileRef, p.Format, p.Language, p.WindowStartMs, p.WindowEndMs, _clock.UtcNow),
            "subtitle_created", "content.media.core_object.subtitle.create_failed", ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateSubtitleRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new UpdateSubtitleCommand(p.SubtitleId, p.OutputRef, _clock.UtcNow), "subtitle_updated", "content.media.core_object.subtitle.update_failed", ct);
    }

    [HttpPost("finalize")]
    public Task<IActionResult> Finalize([FromBody] ApiRequest<FinalizeSubtitleRequestModel> request, CancellationToken ct)
        => Dispatch(new FinalizeSubtitleCommand(request.Data.SubtitleId, _clock.UtcNow), "subtitle_finalized", "content.media.core_object.subtitle.finalize_failed", ct);

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ArchiveSubtitleRequestModel> request, CancellationToken ct)
        => Dispatch(new ArchiveSubtitleCommand(request.Data.SubtitleId, _clock.UtcNow), "subtitle_archived", "content.media.core_object.subtitle.archive_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_media_core_object_subtitle.subtitle_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.media.core_object.subtitle.not_found", $"Subtitle {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<SubtitleReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize SubtitleReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, SubtitleRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record CreateSubtitleRequestModel(Guid AssetRef, Guid? SourceFileRef, string Format, string Language, long? WindowStartMs, long? WindowEndMs);
public sealed record UpdateSubtitleRequestModel(Guid SubtitleId, Guid OutputRef);
public sealed record FinalizeSubtitleRequestModel(Guid SubtitleId);
public sealed record ArchiveSubtitleRequestModel(Guid SubtitleId);
