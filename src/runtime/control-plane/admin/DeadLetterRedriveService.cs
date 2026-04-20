using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.Admin;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.ControlPlane.Admin;

/// <summary>
/// R4.B / R-ADMIN-REDRIVE-ELIGIBILITY-01 — default implementation of
/// <see cref="IDeadLetterRedriveService"/>. Eligibility gate BEFORE publish:
///
/// <list type="number">
///   <item>entry must exist,</item>
///   <item>entry must not already be reprocessed (one-shot),</item>
///   <item>entry must carry a non-empty source topic and payload.</item>
/// </list>
///
/// On a successful publish the entry is marked reprocessed with the operator
/// identity; on publisher failure the DLQ row is untouched (so a retry is
/// valid). Every branch — accepted, refused, failed — emits an audit event
/// via <see cref="IOperatorActionRecorder"/>.
/// </summary>
public sealed class DeadLetterRedriveService : IDeadLetterRedriveService
{
    public const string ActionType = "dlq.redrive";
    public const string ResourceType = "dead-letter-entry";

    private readonly IDeadLetterStore _deadLetterStore;
    private readonly IDeadLetterRedrivePublisher _publisher;
    private readonly IOperatorActionRecorder _auditRecorder;
    private readonly ICallerIdentityAccessor _callerIdentity;
    private readonly IRequestCorrelationAccessor _correlationAccessor;
    private readonly IClock _clock;

    public DeadLetterRedriveService(
        IDeadLetterStore deadLetterStore,
        IDeadLetterRedrivePublisher publisher,
        IOperatorActionRecorder auditRecorder,
        ICallerIdentityAccessor callerIdentity,
        IRequestCorrelationAccessor correlationAccessor,
        IClock clock)
    {
        _deadLetterStore = deadLetterStore;
        _publisher = publisher;
        _auditRecorder = auditRecorder;
        _callerIdentity = callerIdentity;
        _correlationAccessor = correlationAccessor;
        _clock = clock;
    }

    public async Task<DeadLetterRedriveResult> RedriveAsync(
        Guid eventId,
        string operatorIdentityId,
        string? rationale,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(operatorIdentityId))
            throw new ArgumentException("operatorIdentityId is required.", nameof(operatorIdentityId));

        var tenantId = _callerIdentity.GetTenantId();
        var correlationId = _correlationAccessor.Current;

        var entry = await _deadLetterStore.GetAsync(eventId, cancellationToken);
        if (entry is null)
            return await RefusedAsync(
                DeadLetterRedriveOutcome.NotFound, eventId, sourceTopic: null,
                operatorIdentityId, tenantId, correlationId, rationale,
                failureReason: $"No dead-letter entry with event_id={eventId}.",
                cancellationToken);

        if (entry.ReprocessedAt is not null)
            return await RefusedAsync(
                DeadLetterRedriveOutcome.AlreadyReprocessed, eventId, entry.SourceTopic,
                operatorIdentityId, tenantId, correlationId, rationale,
                failureReason: $"Entry already reprocessed at {entry.ReprocessedAt:O} by {entry.ReprocessedByIdentityId ?? "<unknown>"}.",
                cancellationToken);

        if (string.IsNullOrWhiteSpace(entry.SourceTopic) || entry.Payload.Length == 0)
            return await RefusedAsync(
                DeadLetterRedriveOutcome.Ineligible, eventId, entry.SourceTopic,
                operatorIdentityId, tenantId, correlationId, rationale,
                failureReason: "Entry is missing source topic or payload bytes required for re-publication.",
                cancellationToken);

        try
        {
            await _publisher.PublishAsync(
                entry.SourceTopic,
                entry.Payload,
                entry.EventId,
                entry.EventType,
                entry.SchemaVersion,
                entry.CorrelationId,
                cancellationToken);
        }
        catch (DeadLetterRedrivePublishException ex)
        {
            var audit = await _auditRecorder.RecordAsync(
                ActionType, eventId, ResourceType,
                operatorIdentityId, tenantId, correlationId,
                OperatorActionOutcomes.Failed, rationale,
                failureReason: ex.Message, cancellationToken);
            return new DeadLetterRedriveResult
            {
                Outcome = DeadLetterRedriveOutcome.PublishFailed,
                EventId = eventId,
                SourceTopic = entry.SourceTopic,
                FailureReason = ex.Message,
                AuditEventId = audit.EventId,
            };
        }

        await _deadLetterStore.MarkReprocessedAsync(
            eventId, operatorIdentityId, _clock.UtcNow, cancellationToken);

        var acceptedAudit = await _auditRecorder.RecordAsync(
            ActionType, eventId, ResourceType,
            operatorIdentityId, tenantId, correlationId,
            OperatorActionOutcomes.Accepted, rationale,
            failureReason: null, cancellationToken);

        return new DeadLetterRedriveResult
        {
            Outcome = DeadLetterRedriveOutcome.Accepted,
            EventId = eventId,
            SourceTopic = entry.SourceTopic,
            AuditEventId = acceptedAudit.EventId,
        };
    }

    private async Task<DeadLetterRedriveResult> RefusedAsync(
        DeadLetterRedriveOutcome outcome,
        Guid eventId,
        string? sourceTopic,
        string operatorIdentityId,
        string tenantId,
        Guid correlationId,
        string? rationale,
        string failureReason,
        CancellationToken cancellationToken)
    {
        var audit = await _auditRecorder.RecordAsync(
            ActionType, eventId, ResourceType,
            operatorIdentityId, tenantId, correlationId,
            OperatorActionOutcomes.Refused, rationale,
            failureReason: failureReason, cancellationToken);
        return new DeadLetterRedriveResult
        {
            Outcome = outcome,
            EventId = eventId,
            SourceTopic = sourceTopic,
            FailureReason = failureReason,
            AuditEventId = audit.EventId,
        };
    }
}
