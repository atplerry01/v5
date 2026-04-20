namespace Whycespace.Shared.Contracts.Infrastructure.Policy;

public interface IPolicyEvaluator
{
    Task<PolicyDecision> EvaluateAsync(string policyId, object command, PolicyContext policyContext);
}

public sealed record PolicyDecision(
    bool IsAllowed,
    string PolicyId,
    string DecisionHash,
    string? DenialReason)
{
    /// <summary>
    /// R2.A.2 / POL-FAIL-CLASS-01 — classification of the evaluator's
    /// inability to produce a deterministic allow/deny (OPA outage,
    /// timeout, network error). NULL for normal allow/deny decisions
    /// — preserves pre-R2.A.2 behavior and backward-compat for callers
    /// that haven't been uplifted. When non-null, <see cref="IsAllowed"/>
    /// MUST be <c>false</c> and the runtime retry / degraded-mode pathway
    /// handles the decision according to the classification.
    /// </summary>
    public PolicyFailureMode? FailureMode { get; init; }
}

/// <summary>
/// Per-evaluation context forwarded to <see cref="IPolicyEvaluator"/>.
///
/// <para>
/// Pre-B6 shape (positional parameters) carries the identity + route +
/// subject attribute metadata. Phase 8 B6 adds three OPTIONAL init-only
/// properties — <see cref="Command"/>, <see cref="ResourceState"/>,
/// <see cref="Now"/> — that let the OPA adapter populate
/// <c>input.command</c>, <c>input.resource.state</c>, and
/// <c>input.now</c> / <c>input.now_ns</c> uniformly. When these are
/// <c>null</c>, the adapter falls back to the pre-B6 minimal input shape
/// exactly, preserving backward-compat for callers that have not yet been
/// uplifted.
/// </para>
/// </summary>
public sealed record PolicyContext(
    Guid CorrelationId,
    string TenantId,
    string ActorId,
    string CommandType,
    string[] Roles,
    string Classification,
    string Context,
    string Domain,
    IReadOnlyDictionary<string, object>? SubjectAttributes = null)
{
    /// <summary>
    /// Phase 8 B6 — the typed command record as the engine will receive
    /// it. Serialised into <c>input.command</c> on the OPA wire with the
    /// canonical snake-case naming policy so rego rules can dereference
    /// <c>input.command.amount</c>, <c>input.command.counterparty_id</c>,
    /// etc. uniformly. <c>null</c> preserves pre-B6 behaviour (field
    /// omitted from the OPA payload).
    /// </summary>
    public object? Command { get; init; }

    /// <summary>
    /// Phase 8 B6 — the policy-visible aggregate snapshot hydrated via
    /// <see cref="IAggregateStateLoader"/>. Serialised into
    /// <c>input.resource.state</c>. <c>null</c> is passed through as
    /// <c>null</c> on the wire (NOT omitted) so rego rules see an
    /// explicit "no aggregate yet" signal via
    /// <c>not input.resource.state</c>.
    /// </summary>
    public object? ResourceState { get; init; }

    /// <summary>
    /// Phase 8 B6 — the single <see cref="DateTimeOffset"/> read from
    /// <c>IClock.UtcNow</c> at the top of policy evaluation. Threaded
    /// through as both <c>input.now</c> (ISO 8601, human-readable) and
    /// <c>input.now_ns</c> (epoch nanoseconds, rego-friendly numeric
    /// comparison). <c>null</c> omits both fields from the payload.
    /// </summary>
    public DateTimeOffset? Now { get; init; }

    /// <summary>
    /// Phase 8 B6 — the aggregate id resolved from the command's
    /// <c>IHasAggregateId</c>. Serialised into <c>input.resource.aggregate_id</c>
    /// so rego rules can correlate the subject of the decision with the
    /// state snapshot. <c>null</c> omits the field.
    /// </summary>
    public Guid? AggregateId { get; init; }

    /// <summary>
    /// R1 §6 — deployment environment identifier (e.g. "prod", "staging", "dev-eu",
    /// "dr-site-a"). Serialised into <c>input.environment</c>. Lets rego policies
    /// carry environment-scoped overlays (e.g. stricter thresholds in prod, wider
    /// limits in dev). <c>null</c> omits the field.
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// R1 §6 — jurisdiction overlay identifier (e.g. "US-CA", "EU-DE", "NG").
    /// Serialised into <c>input.jurisdiction</c>. Lets rego policies carry
    /// jurisdiction-scoped rules (regulatory thresholds, sanctioned-region gates).
    /// <c>null</c> omits the field.
    /// </summary>
    public string? Jurisdiction { get; init; }
}
