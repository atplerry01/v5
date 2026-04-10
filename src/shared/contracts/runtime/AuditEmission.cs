namespace Whyce.Shared.Contracts.Runtime;

/// <summary>
/// Routing override for audit events that must be persisted to a stream
/// distinct from the command's domain aggregate stream (e.g. policy decision
/// events). Carried on <see cref="CommandResult.AuditEmission"/>.
///
/// Why a dedicated stream: domain aggregate replay must not encounter foreign
/// audit events. Aggregate streams are owned by their aggregate type — a
/// PolicyEvaluatedEvent persisted under a Todo aggregate would pollute Todo
/// replay and risk invariant violations. Audit events get their own stream
/// keyed off a deterministic id derived from the operation coordinates.
///
/// Why dedicated routing metadata: the same separation must hold for the
/// outbound topic. Audit events publish to a constitutional topic
/// (e.g. <c>whyce.constitutional.policy.decision.events</c>) so that
/// audit consumers can subscribe without joining every domain topic.
///
/// All fields are required: the audit emission MUST be self-describing so the
/// fabric can route without inspecting event types (rule 11.R-DOM-01 forbids
/// runtime layer from referencing concrete domain types).
/// </summary>
public sealed record AuditEmission
{
    public required IReadOnlyList<object> Events { get; init; }
    public required Guid AggregateId { get; init; }
    public required string Classification { get; init; }
    public required string Context { get; init; }
    public required string Domain { get; init; }

    /// <summary>
    /// Audit metadata. MUST include DecisionHash, ExecutionHash, PolicyVersion,
    /// CommandId per WBSM v3.5 policy eventification contract. Sourced entirely
    /// from upstream context — no clock, no RNG, no timestamps in any value
    /// that contributes to deterministic hashes.
    /// </summary>
    public required IReadOnlyDictionary<string, string> Metadata { get; init; }
}
