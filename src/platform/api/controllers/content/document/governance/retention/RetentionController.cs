using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Document.Governance.Retention;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Document.Governance.Retention;

[Authorize]
[ApiController]
[Route("api/content/document/governance/retention")]
[ApiExplorerSettings(GroupName = "content.document.governance.retention")]
public sealed class RetentionController : ControllerBase
{
    private static readonly DomainRoute RetentionRoute = new("content", "document", "retention");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public RetentionController(
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

    [HttpPost("apply")]
    public Task<IActionResult> Apply([FromBody] ApiRequest<ApplyRetentionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var retentionId = _idGenerator.Generate($"content:document:governance:retention:{p.TargetKind}:{p.TargetId}:{p.WindowAppliedAt:O}");
        var cmd = new ApplyRetentionCommand(
            retentionId,
            p.TargetId,
            p.TargetKind,
            p.WindowAppliedAt,
            p.WindowExpiresAt,
            p.Reason,
            _clock.UtcNow);
        return Dispatch(cmd, "retention_applied", "content.document.governance.retention.apply_failed", ct);
    }

    [HttpPost("place-hold")]
    public Task<IActionResult> PlaceHold([FromBody] ApiRequest<PlaceRetentionHoldRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new PlaceRetentionHoldCommand(p.RetentionId, p.Reason, _clock.UtcNow);
        return Dispatch(cmd, "retention_hold_placed", "content.document.governance.retention.place_hold_failed", ct);
    }

    [HttpPost("release")]
    public Task<IActionResult> Release([FromBody] ApiRequest<ReleaseRetentionRequestModel> request, CancellationToken ct)
    {
        var cmd = new ReleaseRetentionCommand(request.Data.RetentionId, _clock.UtcNow);
        return Dispatch(cmd, "retention_released", "content.document.governance.retention.release_failed", ct);
    }

    [HttpPost("expire")]
    public Task<IActionResult> Expire([FromBody] ApiRequest<ExpireRetentionRequestModel> request, CancellationToken ct)
    {
        var cmd = new ExpireRetentionCommand(request.Data.RetentionId, _clock.UtcNow);
        return Dispatch(cmd, "retention_expired", "content.document.governance.retention.expire_failed", ct);
    }

    [HttpPost("mark-eligible-for-destruction")]
    public Task<IActionResult> MarkEligibleForDestruction([FromBody] ApiRequest<MarkRetentionEligibleForDestructionRequestModel> request, CancellationToken ct)
    {
        var cmd = new MarkRetentionEligibleForDestructionCommand(request.Data.RetentionId, _clock.UtcNow);
        return Dispatch(cmd, "retention_marked_eligible_for_destruction", "content.document.governance.retention.mark_eligible_for_destruction_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ArchiveRetentionRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveRetentionCommand(request.Data.RetentionId, _clock.UtcNow);
        return Dispatch(cmd, "retention_archived", "content.document.governance.retention.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRetention(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_content_document_governance_retention.retention_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.document.governance.retention.not_found", $"Retention {id} not found.", _clock.UtcNow));

        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<RetentionReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize RetentionReadModel for aggregate {id}.");

        return Ok(ApiResponse.Ok(model, RequestCorrelationId(), _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, RetentionRoute, ct);
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

public sealed record ApplyRetentionRequestModel(
    Guid TargetId,
    string TargetKind,
    DateTimeOffset WindowAppliedAt,
    DateTimeOffset WindowExpiresAt,
    string Reason);

public sealed record PlaceRetentionHoldRequestModel(Guid RetentionId, string Reason);
public sealed record ReleaseRetentionRequestModel(Guid RetentionId);
public sealed record ExpireRetentionRequestModel(Guid RetentionId);
public sealed record MarkRetentionEligibleForDestructionRequestModel(Guid RetentionId);
public sealed record ArchiveRetentionRequestModel(Guid RetentionId);
