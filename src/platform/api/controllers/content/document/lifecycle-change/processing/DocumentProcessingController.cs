using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Processing;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Document.LifecycleChange.Processing;

[Authorize]
[ApiController]
[Route("api/content/document/lifecycle-change/processing")]
[ApiExplorerSettings(GroupName = "content.document.lifecycle_change.processing")]
public sealed class DocumentProcessingController : ControllerBase
{
    private static readonly DomainRoute ProcessingRoute = new("content", "document", "processing");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public DocumentProcessingController(
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
    public Task<IActionResult> RequestProcessing([FromBody] ApiRequest<RequestDocumentProcessingRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var jobId = _idGenerator.Generate($"content:document:lifecycle-change:processing:{p.InputRef}:{p.Kind}");
        var cmd = new RequestDocumentProcessingCommand(jobId, p.Kind, p.InputRef, _clock.UtcNow);
        return Dispatch(cmd, "document_processing_requested", "content.document.lifecycle_change.processing.request_failed", ct);
    }

    [HttpPost("start")]
    public Task<IActionResult> Start([FromBody] ApiRequest<StartDocumentProcessingRequestModel> request, CancellationToken ct)
    {
        var cmd = new StartDocumentProcessingCommand(request.Data.JobId, _clock.UtcNow);
        return Dispatch(cmd, "document_processing_started", "content.document.lifecycle_change.processing.start_failed", ct);
    }

    [HttpPost("complete")]
    public Task<IActionResult> Complete([FromBody] ApiRequest<CompleteDocumentProcessingRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CompleteDocumentProcessingCommand(p.JobId, p.OutputRef, _clock.UtcNow);
        return Dispatch(cmd, "document_processing_completed", "content.document.lifecycle_change.processing.complete_failed", ct);
    }

    [HttpPost("fail")]
    public Task<IActionResult> Fail([FromBody] ApiRequest<FailDocumentProcessingRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new FailDocumentProcessingCommand(p.JobId, p.Reason, _clock.UtcNow);
        return Dispatch(cmd, "document_processing_failed", "content.document.lifecycle_change.processing.fail_failed", ct);
    }

    [HttpPost("cancel")]
    public Task<IActionResult> Cancel([FromBody] ApiRequest<CancelDocumentProcessingRequestModel> request, CancellationToken ct)
    {
        var cmd = new CancelDocumentProcessingCommand(request.Data.JobId, _clock.UtcNow);
        return Dispatch(cmd, "document_processing_cancelled", "content.document.lifecycle_change.processing.cancel_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProcessing(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_content_document_lifecycle_change_processing.document_processing_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.document.lifecycle_change.processing.not_found", $"DocumentProcessing {id} not found.", _clock.UtcNow));

        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<DocumentProcessingReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize DocumentProcessingReadModel for aggregate {id}.");

        return Ok(ApiResponse.Ok(model, RequestCorrelationId(), _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, ProcessingRoute, ct);
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

public sealed record RequestDocumentProcessingRequestModel(string Kind, Guid InputRef);
public sealed record StartDocumentProcessingRequestModel(Guid JobId);
public sealed record CompleteDocumentProcessingRequestModel(Guid JobId, Guid OutputRef);
public sealed record FailDocumentProcessingRequestModel(Guid JobId, string Reason);
public sealed record CancelDocumentProcessingRequestModel(Guid JobId);
