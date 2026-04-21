using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Document.Intake.Upload;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Document.Intake.Upload;

[Authorize]
[ApiController]
[Route("api/content/document/intake/upload")]
[ApiExplorerSettings(GroupName = "content.document.intake.upload")]
public sealed class DocumentUploadController : ControllerBase
{
    private static readonly DomainRoute UploadRoute = new("content", "document", "upload");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public DocumentUploadController(
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

    [HttpPost("request")]
    public Task<IActionResult> RequestUpload([FromBody] ApiRequest<RequestDocumentUploadRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var uploadId = _idGenerator.Generate($"content:document:intake:upload:{p.SourceRef}:{p.InputRef}");
        var cmd = new RequestDocumentUploadCommand(uploadId, p.SourceRef, p.InputRef, _clock.UtcNow);
        return Dispatch(cmd, "document_upload_requested", "content.document.intake.upload.request_failed", ct);
    }

    [HttpPost("accept")]
    public Task<IActionResult> Accept([FromBody] ApiRequest<AcceptDocumentUploadRequestModel> request, CancellationToken ct)
    {
        var cmd = new AcceptDocumentUploadCommand(request.Data.UploadId, _clock.UtcNow);
        return Dispatch(cmd, "document_upload_accepted", "content.document.intake.upload.accept_failed", ct);
    }

    [HttpPost("start-processing")]
    public Task<IActionResult> StartProcessing([FromBody] ApiRequest<StartDocumentUploadProcessingRequestModel> request, CancellationToken ct)
    {
        var cmd = new StartDocumentUploadProcessingCommand(request.Data.UploadId, _clock.UtcNow);
        return Dispatch(cmd, "document_upload_processing_started", "content.document.intake.upload.start_processing_failed", ct);
    }

    [HttpPost("complete")]
    public Task<IActionResult> Complete([FromBody] ApiRequest<CompleteDocumentUploadRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CompleteDocumentUploadCommand(p.UploadId, p.OutputRef, _clock.UtcNow);
        return Dispatch(cmd, "document_upload_completed", "content.document.intake.upload.complete_failed", ct);
    }

    [HttpPost("fail")]
    public Task<IActionResult> Fail([FromBody] ApiRequest<FailDocumentUploadRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new FailDocumentUploadCommand(p.UploadId, p.Reason, _clock.UtcNow);
        return Dispatch(cmd, "document_upload_failed", "content.document.intake.upload.fail_failed", ct);
    }

    [HttpPost("cancel")]
    public Task<IActionResult> Cancel([FromBody] ApiRequest<CancelDocumentUploadRequestModel> request, CancellationToken ct)
    {
        var cmd = new CancelDocumentUploadCommand(request.Data.UploadId, _clock.UtcNow);
        return Dispatch(cmd, "document_upload_cancelled", "content.document.intake.upload.cancel_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUpload(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_content_document_intake_upload.document_upload_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.document.intake.upload.not_found", $"DocumentUpload {id} not found.", _clock.UtcNow));

        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<DocumentUploadReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize DocumentUploadReadModel for aggregate {id}.");

        return Ok(ApiResponse.Ok(model, RequestCorrelationId(), _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, UploadRoute, ct);
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

public sealed record RequestDocumentUploadRequestModel(Guid SourceRef, Guid InputRef);
public sealed record AcceptDocumentUploadRequestModel(Guid UploadId);
public sealed record StartDocumentUploadProcessingRequestModel(Guid UploadId);
public sealed record CompleteDocumentUploadRequestModel(Guid UploadId, Guid OutputRef);
public sealed record FailDocumentUploadRequestModel(Guid UploadId, string Reason);
public sealed record CancelDocumentUploadRequestModel(Guid UploadId);
