using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Platform.Api.Controllers.Platform.Admin.Shared;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.Admin;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Admin.Dlq;

/// <summary>
/// R4.B — admin inspection + re-drive surface for dead-letter entries.
/// Inspection is a bounded query against <see cref="IDeadLetterStore"/>
/// (limit capped at 1000 by the store contract). The re-drive path
/// delegates to <see cref="IDeadLetterRedriveService"/> which owns the
/// eligibility gate, publisher coordination, reprocess-mark, and audit
/// emission — this controller never touches the DLQ store or a Kafka
/// producer directly.
/// </summary>
[ApiExplorerSettings(GroupName = "platform.admin.dlq")]
[Route(AdminScope.RoutePrefix + "/dlq")]
public sealed class DlqAdminController : AdminControllerBase
{
    private readonly IDeadLetterStore _deadLetterStore;
    private readonly IDeadLetterRedriveService _redriveService;

    public DlqAdminController(
        IDeadLetterStore deadLetterStore,
        IDeadLetterRedriveService redriveService,
        ICallerIdentityAccessor callerIdentity,
        IOperatorActionRecorder auditRecorder,
        IClock clock) : base(callerIdentity, auditRecorder, clock)
    {
        _deadLetterStore = deadLetterStore;
        _redriveService = redriveService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? sourceTopic,
        [FromQuery] DateTimeOffset? since,
        [FromQuery] int limit = 100,
        [FromQuery] bool includeReprocessed = false,
        CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(sourceTopic))
        {
            var byTopic = await _deadLetterStore.ListAsync(sourceTopic, since, limit, ct);
            return Ok(byTopic.Select(ToProjection));
        }

        var all = await _deadLetterStore.ListAllAsync(since, limit, includeReprocessed, ct);
        return Ok(all.Select(ToProjection));
    }

    [HttpGet("{eventId:guid}")]
    public async Task<IActionResult> Get(Guid eventId, CancellationToken ct)
    {
        var entry = await _deadLetterStore.GetAsync(eventId, ct);
        return entry is null
            ? Refused("platform.admin.dlq.not_found", $"DeadLetterEntry {eventId} not found.", StatusCodes.Status404NotFound)
            : Ok(ToProjection(entry));
    }

    [HttpPost("{eventId:guid}/redrive")]
    public async Task<IActionResult> Redrive(
        Guid eventId,
        [FromBody] RedriveDlqRequest request,
        CancellationToken ct)
    {
        var result = await _redriveService.RedriveAsync(
            eventId, OperatorIdentityId(), request.Rationale, ct);

        return result.Outcome switch
        {
            DeadLetterRedriveOutcome.Accepted => Ok(new RedriveDlqResponse(
                eventId, result.SourceTopic, result.AuditEventId, Status: "accepted")),
            DeadLetterRedriveOutcome.NotFound => Refused(
                "platform.admin.dlq.redrive.not_found",
                result.FailureReason ?? "Entry not found.",
                StatusCodes.Status404NotFound),
            DeadLetterRedriveOutcome.AlreadyReprocessed => Refused(
                "platform.admin.dlq.redrive.already_reprocessed",
                result.FailureReason ?? "Entry already reprocessed.",
                StatusCodes.Status409Conflict),
            DeadLetterRedriveOutcome.Ineligible => Refused(
                "platform.admin.dlq.redrive.ineligible",
                result.FailureReason ?? "Entry is ineligible for re-drive."),
            DeadLetterRedriveOutcome.PublishFailed => Refused(
                "platform.admin.dlq.redrive.publish_failed",
                result.FailureReason ?? "Publish failed.",
                StatusCodes.Status502BadGateway),
            _ => Refused("platform.admin.dlq.redrive.unknown_outcome", "Unclassified result."),
        };
    }

    // Strip the raw payload bytes from the admin projection — operators get
    // the metadata they need to decide (failure category, attempt count, time,
    // reprocess state, schema version) without the store streaming every
    // poison-message body into their listing. Individual GET can still return
    // the payload in a follow-up iteration if needed; for R4.B we keep the
    // inspection surface narrow.
    private static DlqEntryProjection ToProjection(DeadLetterEntry e) => new(
        EventId: e.EventId,
        SourceTopic: e.SourceTopic,
        EventType: e.EventType,
        CorrelationId: e.CorrelationId,
        CausationId: e.CausationId,
        EnqueuedAt: e.EnqueuedAt,
        FailureCategory: e.FailureCategory?.ToString(),
        LastError: e.LastError,
        AttemptCount: e.AttemptCount,
        PayloadSize: e.Payload.Length,
        SchemaVersion: e.SchemaVersion,
        ReprocessedAt: e.ReprocessedAt,
        ReprocessedByIdentityId: e.ReprocessedByIdentityId);
}

public sealed record RedriveDlqRequest(string? Rationale);

public sealed record RedriveDlqResponse(
    Guid EventId,
    string? SourceTopic,
    Guid? AuditEventId,
    string Status);

public sealed record DlqEntryProjection(
    Guid EventId,
    string SourceTopic,
    string EventType,
    Guid CorrelationId,
    Guid? CausationId,
    DateTimeOffset EnqueuedAt,
    string? FailureCategory,
    string LastError,
    int AttemptCount,
    int PayloadSize,
    int? SchemaVersion,
    DateTimeOffset? ReprocessedAt,
    string? ReprocessedByIdentityId);
