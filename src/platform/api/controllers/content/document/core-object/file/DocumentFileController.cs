using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.File;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Document.CoreObject.File;

[Authorize]
[ApiController]
[Route("api/content/document/core-object/file")]
[ApiExplorerSettings(GroupName = "content.document.core_object.file")]
public sealed class DocumentFileController : ControllerBase
{
    private static readonly DomainRoute FileRoute = new("content", "document", "file");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public DocumentFileController(
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

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterDocumentFileRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var fileId = _idGenerator.Generate($"content:document:core-object:file:{p.DocumentId}:{p.DeclaredChecksum}");
        var cmd = new RegisterDocumentFileCommand(
            fileId,
            p.DocumentId,
            p.StorageRef,
            p.DeclaredChecksum,
            p.MimeType,
            p.SizeBytes,
            _clock.UtcNow);
        return Dispatch(cmd, "document_file_registered", "content.document.core_object.file.register_failed", ct);
    }

    [HttpPost("verify-integrity")]
    public Task<IActionResult> VerifyIntegrity([FromBody] ApiRequest<VerifyDocumentFileIntegrityRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new VerifyDocumentFileIntegrityCommand(p.DocumentFileId, p.ComputedChecksum, _clock.UtcNow);
        return Dispatch(cmd, "document_file_integrity_verified", "content.document.core_object.file.verify_integrity_failed", ct);
    }

    [HttpPost("supersede")]
    public Task<IActionResult> Supersede([FromBody] ApiRequest<SupersedeDocumentFileRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new SupersedeDocumentFileCommand(p.DocumentFileId, p.SuccessorFileId, _clock.UtcNow);
        return Dispatch(cmd, "document_file_superseded", "content.document.core_object.file.supersede_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ArchiveDocumentFileRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveDocumentFileCommand(request.Data.DocumentFileId, _clock.UtcNow);
        return Dispatch(cmd, "document_file_archived", "content.document.core_object.file.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetFile(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_content_document_core_object_file.document_file_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.document.core_object.file.not_found", $"DocumentFile {id} not found.", _clock.UtcNow));

        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<DocumentFileReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize DocumentFileReadModel for aggregate {id}.");

        return Ok(ApiResponse.Ok(model, RequestCorrelationId(), _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, FileRoute, ct);
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

public sealed record RegisterDocumentFileRequestModel(
    Guid DocumentId,
    string StorageRef,
    string DeclaredChecksum,
    string MimeType,
    long SizeBytes);

public sealed record VerifyDocumentFileIntegrityRequestModel(Guid DocumentFileId, string ComputedChecksum);
public sealed record SupersedeDocumentFileRequestModel(Guid DocumentFileId, Guid SuccessorFileId);
public sealed record ArchiveDocumentFileRequestModel(Guid DocumentFileId);
