namespace Whycespace.Shared.Contracts.Runtime.Admin;

/// <summary>
/// R4.B / R-ADMIN-AUDIT-EMISSION-01 — sanctioned seam for emitting
/// <see cref="OperatorActionEvent"/> evidence. Every mutating admin/operator
/// action MUST invoke <see cref="RecordAsync"/> exactly once — on accepted,
/// refused, AND failed outcomes — so every attempt leaves a trail.
///
/// <para>The recorder is responsible for fabric routing to the runtime
/// operator-action audit stream and for computing the deterministic event
/// id. Callers never construct the event id directly.</para>
/// </summary>
public interface IOperatorActionRecorder
{
    /// <summary>
    /// Record an operator action to the audit stream. Idempotent on the
    /// deterministic event id derived from
    /// <c>{CorrelationId}:{ActionType}:{TargetId}</c>. Every input is
    /// required so the emitted event carries a complete evidence set.
    /// </summary>
    Task<OperatorActionEvent> RecordAsync(
        string actionType,
        Guid targetId,
        string targetResourceType,
        string operatorIdentityId,
        string tenantId,
        Guid correlationId,
        string outcome,
        string? rationale = null,
        string? failureReason = null,
        CancellationToken cancellationToken = default);
}
