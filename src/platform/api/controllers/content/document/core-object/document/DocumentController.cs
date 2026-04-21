using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Document;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Document.CoreObject.Document;

[Authorize]
[ApiController]
[Route("api/content/document/core-object/document")]
[ApiExplorerSettings(GroupName = "content.document.core_object.document")]
public sealed class DocumentController : ControllerBase
{
    private static readonly DomainRoute DocumentRoute = new("content", "document", "document");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public DocumentController(
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
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateDocumentRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var documentId = _idGenerator.Generate($"content:document:core-object:document:{p.StructuralOwnerId}:{p.Title}");
        var cmd = new CreateDocumentCommand(
            documentId,
            p.Title,
            p.Type,
            p.Classification,
            p.StructuralOwnerId,
            p.BusinessOwnerKind,
            p.BusinessOwnerId,
            _clock.UtcNow);
        return Dispatch(cmd, "document_created", "content.document.core_object.document.create_failed", ct);
    }

    [HttpPost("update-metadata")]
    public Task<IActionResult> UpdateMetadata([FromBody] ApiRequest<UpdateDocumentMetadataRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateDocumentMetadataCommand(p.DocumentId, p.NewTitle, p.NewType, p.NewClassification, _clock.UtcNow);
        return Dispatch(cmd, "document_metadata_updated", "content.document.core_object.document.update_metadata_failed", ct);
    }

    [HttpPost("attach-version")]
    public Task<IActionResult> AttachVersion([FromBody] ApiRequest<AttachDocumentVersionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AttachDocumentVersionCommand(p.DocumentId, p.VersionId, _clock.UtcNow);
        return Dispatch(cmd, "document_version_attached", "content.document.core_object.document.attach_version_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateDocumentRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateDocumentCommand(request.Data.DocumentId, _clock.UtcNow);
        return Dispatch(cmd, "document_activated", "content.document.core_object.document.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ArchiveDocumentRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveDocumentCommand(request.Data.DocumentId, _clock.UtcNow);
        return Dispatch(cmd, "document_archived", "content.document.core_object.document.archive_failed", ct);
    }

    [HttpPost("restore")]
    public Task<IActionResult> Restore([FromBody] ApiRequest<RestoreDocumentRequestModel> request, CancellationToken ct)
    {
        var cmd = new RestoreDocumentCommand(request.Data.DocumentId, _clock.UtcNow);
        return Dispatch(cmd, "document_restored", "content.document.core_object.document.restore_failed", ct);
    }

    [HttpPost("supersede")]
    public Task<IActionResult> Supersede([FromBody] ApiRequest<SupersedeDocumentRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new SupersedeDocumentCommand(p.DocumentId, p.SupersedingDocumentId, _clock.UtcNow);
        return Dispatch(cmd, "document_superseded", "content.document.core_object.document.supersede_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDocument(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_content_document_core_object_document.document_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.document.core_object.document.not_found", $"Document {id} not found.", _clock.UtcNow));

        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<DocumentReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize DocumentReadModel for aggregate {id}.");

        return Ok(ApiResponse.Ok(model, RequestCorrelationId(), _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, DocumentRoute, ct);
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

public sealed record CreateDocumentRequestModel(
    string Title,
    string Type,
    string Classification,
    Guid StructuralOwnerId,
    string BusinessOwnerKind,
    Guid BusinessOwnerId);

public sealed record UpdateDocumentMetadataRequestModel(
    Guid DocumentId,
    string NewTitle,
    string NewType,
    string NewClassification);

public sealed record AttachDocumentVersionRequestModel(Guid DocumentId, Guid VersionId);
public sealed record ActivateDocumentRequestModel(Guid DocumentId);
public sealed record ArchiveDocumentRequestModel(Guid DocumentId);
public sealed record RestoreDocumentRequestModel(Guid DocumentId);
public sealed record SupersedeDocumentRequestModel(Guid DocumentId, Guid SupersedingDocumentId);
