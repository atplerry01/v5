using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Document.LifecycleChange.Version;

[Authorize]
[ApiController]
[Route("api/content/document/lifecycle-change/version")]
[ApiExplorerSettings(GroupName = "content.document.lifecycle_change.version")]
public sealed class DocumentVersionController : ControllerBase
{
    private static readonly DomainRoute VersionRoute = new("content", "document", "version");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public DocumentVersionController(
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
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateDocumentVersionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var versionId = _idGenerator.Generate($"content:document:lifecycle-change:version:{p.DocumentRef}:{p.Major}:{p.Minor}");
        var cmd = new CreateDocumentVersionCommand(
            versionId,
            p.DocumentRef,
            p.Major,
            p.Minor,
            p.ArtifactRef,
            p.PreviousVersionId,
            _clock.UtcNow);
        return Dispatch(cmd, "document_version_created", "content.document.lifecycle_change.version.create_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateDocumentVersionRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateDocumentVersionCommand(request.Data.VersionId, _clock.UtcNow);
        return Dispatch(cmd, "document_version_activated", "content.document.lifecycle_change.version.activate_failed", ct);
    }

    [HttpPost("supersede")]
    public Task<IActionResult> Supersede([FromBody] ApiRequest<SupersedeDocumentVersionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new SupersedeDocumentVersionCommand(p.VersionId, p.SuccessorVersionId, _clock.UtcNow);
        return Dispatch(cmd, "document_version_superseded", "content.document.lifecycle_change.version.supersede_failed", ct);
    }

    [HttpPost("withdraw")]
    public Task<IActionResult> Withdraw([FromBody] ApiRequest<WithdrawDocumentVersionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new WithdrawDocumentVersionCommand(p.VersionId, p.Reason, _clock.UtcNow);
        return Dispatch(cmd, "document_version_withdrawn", "content.document.lifecycle_change.version.withdraw_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetVersion(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_content_document_lifecycle_change_version.document_version_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.document.lifecycle_change.version.not_found", $"DocumentVersion {id} not found.", _clock.UtcNow));

        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<DocumentVersionReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize DocumentVersionReadModel for aggregate {id}.");

        return Ok(ApiResponse.Ok(model, RequestCorrelationId(), _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, VersionRoute, ct);
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

public sealed record CreateDocumentVersionRequestModel(
    Guid DocumentRef,
    int Major,
    int Minor,
    Guid ArtifactRef,
    Guid? PreviousVersionId);

public sealed record ActivateDocumentVersionRequestModel(Guid VersionId);
public sealed record SupersedeDocumentVersionRequestModel(Guid VersionId, Guid SuccessorVersionId);
public sealed record WithdrawDocumentVersionRequestModel(Guid VersionId, string Reason);
