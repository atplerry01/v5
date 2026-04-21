using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Template;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Document.CoreObject.Template;

[Authorize]
[ApiController]
[Route("api/content/document/core-object/template")]
[ApiExplorerSettings(GroupName = "content.document.core_object.template")]
public sealed class DocumentTemplateController : ControllerBase
{
    private static readonly DomainRoute TemplateRoute = new("content", "document", "template");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public DocumentTemplateController(
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
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateDocumentTemplateRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var templateId = _idGenerator.Generate($"content:document:core-object:template:{p.Name}:{p.Type}");
        var cmd = new CreateDocumentTemplateCommand(
            templateId,
            p.Name,
            p.Type,
            p.SchemaRefId,
            _clock.UtcNow);
        return Dispatch(cmd, "document_template_created", "content.document.core_object.template.create_failed", ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateDocumentTemplateRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateDocumentTemplateCommand(p.TemplateId, p.NewName, p.NewType, p.NewSchemaRefId, _clock.UtcNow);
        return Dispatch(cmd, "document_template_updated", "content.document.core_object.template.update_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateDocumentTemplateRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateDocumentTemplateCommand(request.Data.TemplateId, _clock.UtcNow);
        return Dispatch(cmd, "document_template_activated", "content.document.core_object.template.activate_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<DeprecateDocumentTemplateRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DeprecateDocumentTemplateCommand(p.TemplateId, p.Reason, _clock.UtcNow);
        return Dispatch(cmd, "document_template_deprecated", "content.document.core_object.template.deprecate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ArchiveDocumentTemplateRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveDocumentTemplateCommand(request.Data.TemplateId, _clock.UtcNow);
        return Dispatch(cmd, "document_template_archived", "content.document.core_object.template.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTemplate(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_content_document_core_object_template.document_template_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.document.core_object.template.not_found", $"DocumentTemplate {id} not found.", _clock.UtcNow));

        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<DocumentTemplateReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize DocumentTemplateReadModel for aggregate {id}.");

        return Ok(ApiResponse.Ok(model, RequestCorrelationId(), _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, TemplateRoute, ct);
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

public sealed record CreateDocumentTemplateRequestModel(
    string Name,
    string Type,
    Guid? SchemaRefId);

public sealed record UpdateDocumentTemplateRequestModel(
    Guid TemplateId,
    string NewName,
    string NewType,
    Guid? NewSchemaRefId);

public sealed record ActivateDocumentTemplateRequestModel(Guid TemplateId);
public sealed record DeprecateDocumentTemplateRequestModel(Guid TemplateId, string Reason);
public sealed record ArchiveDocumentTemplateRequestModel(Guid TemplateId);
