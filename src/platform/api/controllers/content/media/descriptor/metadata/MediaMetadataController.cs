using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Media.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Media.Descriptor.Metadata;

[Authorize]
[ApiController]
[Route("api/content/media/descriptor/metadata")]
[ApiExplorerSettings(GroupName = "content.media.descriptor.metadata")]
public sealed class MediaMetadataController : ControllerBase
{
    private static readonly DomainRoute MetadataRoute = new("content", "media", "metadata");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public MediaMetadataController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateMediaMetadataRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var metadataId = _idGenerator.Generate($"content:media:descriptor:metadata:{p.AssetRef}");
        return Dispatch(new CreateMediaMetadataCommand(metadataId, p.AssetRef, _clock.UtcNow), "media_metadata_created", "content.media.descriptor.metadata.create_failed", ct);
    }

    [HttpPost("add-entry")]
    public Task<IActionResult> AddEntry([FromBody] ApiRequest<AddMediaMetadataEntryRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new AddMediaMetadataEntryCommand(p.MetadataId, p.Key, p.Value, _clock.UtcNow), "media_metadata_entry_added", "content.media.descriptor.metadata.add_entry_failed", ct);
    }

    [HttpPost("update-entry")]
    public Task<IActionResult> UpdateEntry([FromBody] ApiRequest<UpdateMediaMetadataEntryRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new UpdateMediaMetadataEntryCommand(p.MetadataId, p.Key, p.NewValue, _clock.UtcNow), "media_metadata_entry_updated", "content.media.descriptor.metadata.update_entry_failed", ct);
    }

    [HttpPost("remove-entry")]
    public Task<IActionResult> RemoveEntry([FromBody] ApiRequest<RemoveMediaMetadataEntryRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new RemoveMediaMetadataEntryCommand(p.MetadataId, p.Key, _clock.UtcNow), "media_metadata_entry_removed", "content.media.descriptor.metadata.remove_entry_failed", ct);
    }

    [HttpPost("finalize")]
    public Task<IActionResult> Finalize([FromBody] ApiRequest<FinalizeMediaMetadataRequestModel> request, CancellationToken ct)
        => Dispatch(new FinalizeMediaMetadataCommand(request.Data.MetadataId, _clock.UtcNow), "media_metadata_finalized", "content.media.descriptor.metadata.finalize_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_media_descriptor_metadata.media_metadata_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.media.descriptor.metadata.not_found", $"MediaMetadata {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<MediaMetadataReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize MediaMetadataReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, MetadataRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record CreateMediaMetadataRequestModel(Guid AssetRef);
public sealed record AddMediaMetadataEntryRequestModel(Guid MetadataId, string Key, string Value);
public sealed record UpdateMediaMetadataEntryRequestModel(Guid MetadataId, string Key, string NewValue);
public sealed record RemoveMediaMetadataEntryRequestModel(Guid MetadataId, string Key);
public sealed record FinalizeMediaMetadataRequestModel(Guid MetadataId);
