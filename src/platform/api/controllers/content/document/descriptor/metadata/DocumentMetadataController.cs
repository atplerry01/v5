using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Document.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Document.Descriptor.Metadata;

[Authorize]
[ApiController]
[Route("api/content/document/descriptor/metadata")]
[ApiExplorerSettings(GroupName = "content.document.descriptor.metadata")]
public sealed class DocumentMetadataController : ControllerBase
{
    private static readonly DomainRoute MetadataRoute = new("content", "document", "metadata");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public DocumentMetadataController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _projectionsConnectionString = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException(
                "Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateDocumentMetadataRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var metadataId = _idGenerator.Generate($"content:document:descriptor:metadata:{p.DocumentId}");
        var cmd = new CreateDocumentMetadataCommand(
            metadataId,
            p.DocumentId,
            _clock.UtcNow);
        return Dispatch(cmd, "document_metadata_created", "content.document.descriptor.metadata.create_failed", ct);
    }

    [HttpPost("add-entry")]
    public Task<IActionResult> AddEntry([FromBody] ApiRequest<AddDocumentMetadataEntryRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AddDocumentMetadataEntryCommand(p.MetadataId, p.Key, p.Value, _clock.UtcNow);
        return Dispatch(cmd, "document_metadata_entry_added", "content.document.descriptor.metadata.add_entry_failed", ct);
    }

    [HttpPost("update-entry")]
    public Task<IActionResult> UpdateEntry([FromBody] ApiRequest<UpdateDocumentMetadataEntryRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateDocumentMetadataEntryCommand(p.MetadataId, p.Key, p.NewValue, _clock.UtcNow);
        return Dispatch(cmd, "document_metadata_entry_updated", "content.document.descriptor.metadata.update_entry_failed", ct);
    }

    [HttpPost("remove-entry")]
    public Task<IActionResult> RemoveEntry([FromBody] ApiRequest<RemoveDocumentMetadataEntryRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RemoveDocumentMetadataEntryCommand(p.MetadataId, p.Key, _clock.UtcNow);
        return Dispatch(cmd, "document_metadata_entry_removed", "content.document.descriptor.metadata.remove_entry_failed", ct);
    }

    [HttpPost("finalize")]
    public Task<IActionResult> Finalize([FromBody] ApiRequest<FinalizeDocumentMetadataRequestModel> request, CancellationToken ct)
    {
        var cmd = new FinalizeDocumentMetadataCommand(request.Data.MetadataId, _clock.UtcNow);
        return Dispatch(cmd, "document_metadata_finalized", "content.document.descriptor.metadata.finalize_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMetadata(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_content_document_descriptor_metadata.document_metadata_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.document.descriptor.metadata.not_found", $"DocumentMetadata {id} not found.", _clock.UtcNow));

        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<DocumentMetadataReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize DocumentMetadataReadModel for aggregate {id}.");

        return Ok(ApiResponse.Ok(model, RequestCorrelationId(), _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, MetadataRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }

    private Guid RequestCorrelationId()
    {
        if (HttpContext is { } ctx
            && ctx.Request.Headers.TryGetValue("X-Correlation-Id", out var values)
            && Guid.TryParse(values.ToString(), out var parsed))
        {
            return parsed;
        }
        return Guid.Empty;
    }
}

public sealed record CreateDocumentMetadataRequestModel(Guid DocumentId);
public sealed record AddDocumentMetadataEntryRequestModel(Guid MetadataId, string Key, string Value);
public sealed record UpdateDocumentMetadataEntryRequestModel(Guid MetadataId, string Key, string NewValue);
public sealed record RemoveDocumentMetadataEntryRequestModel(Guid MetadataId, string Key);
public sealed record FinalizeDocumentMetadataRequestModel(Guid MetadataId);
