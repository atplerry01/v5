using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Bundle;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Document.CoreObject.Bundle;

[Authorize]
[ApiController]
[Route("api/content/document/core-object/bundle")]
[ApiExplorerSettings(GroupName = "content.document.core_object.bundle")]
public sealed class DocumentBundleController : ControllerBase
{
    private static readonly DomainRoute BundleRoute = new("content", "document", "bundle");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public DocumentBundleController(
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
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateDocumentBundleRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var bundleId = _idGenerator.Generate($"content:document:core-object:bundle:{p.Name}");
        var cmd = new CreateDocumentBundleCommand(bundleId, p.Name, _clock.UtcNow);
        return Dispatch(cmd, "document_bundle_created", "content.document.core_object.bundle.create_failed", ct);
    }

    [HttpPost("rename")]
    public Task<IActionResult> Rename([FromBody] ApiRequest<RenameDocumentBundleRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RenameDocumentBundleCommand(p.BundleId, p.NewName, _clock.UtcNow);
        return Dispatch(cmd, "document_bundle_renamed", "content.document.core_object.bundle.rename_failed", ct);
    }

    [HttpPost("add-member")]
    public Task<IActionResult> AddMember([FromBody] ApiRequest<AddDocumentBundleMemberRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AddDocumentBundleMemberCommand(p.BundleId, p.MemberId, _clock.UtcNow);
        return Dispatch(cmd, "document_bundle_member_added", "content.document.core_object.bundle.add_member_failed", ct);
    }

    [HttpPost("remove-member")]
    public Task<IActionResult> RemoveMember([FromBody] ApiRequest<RemoveDocumentBundleMemberRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RemoveDocumentBundleMemberCommand(p.BundleId, p.MemberId, _clock.UtcNow);
        return Dispatch(cmd, "document_bundle_member_removed", "content.document.core_object.bundle.remove_member_failed", ct);
    }

    [HttpPost("finalize")]
    public Task<IActionResult> Finalize([FromBody] ApiRequest<FinalizeDocumentBundleRequestModel> request, CancellationToken ct)
    {
        var cmd = new FinalizeDocumentBundleCommand(request.Data.BundleId, _clock.UtcNow);
        return Dispatch(cmd, "document_bundle_finalized", "content.document.core_object.bundle.finalize_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ArchiveDocumentBundleRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveDocumentBundleCommand(request.Data.BundleId, _clock.UtcNow);
        return Dispatch(cmd, "document_bundle_archived", "content.document.core_object.bundle.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBundle(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_content_document_core_object_bundle.document_bundle_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.document.core_object.bundle.not_found", $"Bundle {id} not found.", _clock.UtcNow));

        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<DocumentBundleReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize DocumentBundleReadModel for aggregate {id}.");

        return Ok(ApiResponse.Ok(model, RequestCorrelationId(), _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, BundleRoute, ct);
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

public sealed record CreateDocumentBundleRequestModel(string Name);
public sealed record RenameDocumentBundleRequestModel(Guid BundleId, string NewName);
public sealed record AddDocumentBundleMemberRequestModel(Guid BundleId, Guid MemberId);
public sealed record RemoveDocumentBundleMemberRequestModel(Guid BundleId, Guid MemberId);
public sealed record FinalizeDocumentBundleRequestModel(Guid BundleId);
public sealed record ArchiveDocumentBundleRequestModel(Guid BundleId);
