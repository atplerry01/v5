namespace Whycespace.Shared.Contracts.Runtime.Admin;

/// <summary>
/// R4.B / R-ADMIN-AUDIT-EMISSION-01 — canonical evidence event for every
/// mutating operator action on the admin surface. Persisted on a dedicated
/// runtime audit stream (see <see cref="AdminScope.AuditClassification"/>) so
/// aggregate replay never encounters operator-action envelopes.
///
/// <para>Deterministic: every field is sourced from caller context, request
/// correlation, or <c>IClock</c> — never from RNG. <see cref="OccurredAt"/>
/// is recorded for human-inspection semantics; it does NOT feed any
/// deterministic hash.</para>
/// </summary>
public sealed record OperatorActionEvent
{
    /// <summary>
    /// Deterministic identifier derived from
    /// <c>{CorrelationId}:{ActionType}:{TargetId}</c>. Two calls with the
    /// same coordinates collapse to a single audit row.
    /// </summary>
    public required Guid EventId { get; init; }

    /// <summary>Authenticated operator sub ("who"). Never "system" for admin-surface actions.</summary>
    public required string OperatorIdentityId { get; init; }

    /// <summary>Canonical action identifier (e.g. "outbound-effect.reconcile", "dlq.redrive").</summary>
    public required string ActionType { get; init; }

    /// <summary>Target resource identifier (effect id, dlq event id, workflow id, …).</summary>
    public required Guid TargetId { get; init; }

    /// <summary>Target resource type discriminator, parallel to <see cref="ActionType"/>.</summary>
    public required string TargetResourceType { get; init; }

    /// <summary>Optional free-text rationale supplied by the operator.</summary>
    public string? Rationale { get; init; }

    /// <summary>Outcome classification: "accepted", "refused", "failed".</summary>
    public required string Outcome { get; init; }

    /// <summary>Machine-readable reason for refused/failed outcomes; null on accepted.</summary>
    public string? FailureReason { get; init; }

    /// <summary>Request correlation id — links the action to the HTTP envelope it arrived in.</summary>
    public required Guid CorrelationId { get; init; }

    /// <summary>Tenant id from the authenticated caller.</summary>
    public required string TenantId { get; init; }

    /// <summary>Wall-clock instant recorded via <c>IClock.UtcNow</c>. Observability-only.</summary>
    public required DateTimeOffset OccurredAt { get; init; }
}
