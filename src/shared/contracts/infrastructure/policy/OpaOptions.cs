namespace Whycespace.Shared.Contracts.Infrastructure.Policy;

/// <summary>
/// Tunable behavior for the OPA-backed <see cref="IPolicyEvaluator"/>.
///
/// phase1.5-S5.2.1 / PC-2 (OPA-CONFIG-01): externalizes the previously
/// hardcoded 5 s HttpClient timeout and introduces a configuration-bound
/// circuit breaker on the policy-evaluation hot path. Follows the
/// phase1.6-S1.5 OutboxOptions precedent: a plain record bound by the
/// composition root from <c>IConfiguration</c>, no <c>IOptions&lt;T&gt;</c>
/// indirection, defaults conservative enough that an unconfigured
/// deployment is safer (not the same — failures now refuse rather than
/// hang) than the pre-S5.2.1 behavior.
///
/// Failure semantics are non-allowing by construction: every field on this
/// record affects how a failure is *classified*, never whether a failure
/// becomes an implicit allow. Policy primacy ($8) is preserved.
/// </summary>
public sealed record OpaOptions
{
    /// <summary>
    /// OPA REST endpoint base URL (e.g. <c>http://opa:8181</c>). Required;
    /// the composition root throws when the configuration key is unset.
    /// </summary>
    public string Endpoint { get; init; } = string.Empty;

    /// <summary>
    /// Per-call timeout, in milliseconds, applied via a linked
    /// <see cref="System.Threading.CancellationTokenSource"/> on the OPA
    /// HTTP request. A timeout produces a typed
    /// <see cref="PolicyEvaluationUnavailableException"/> — never an
    /// implicit allow. Default 250 ms (conservative for an in-cluster OPA).
    /// </summary>
    public int RequestTimeoutMs { get; init; } = 250;

    /// <summary>
    /// Number of consecutive failures (timeout, transport, non-2xx) within
    /// <see cref="BreakerWindowSeconds"/> that trip the breaker into the
    /// Open state. Must be at least 1. Default 5.
    /// </summary>
    public int BreakerThreshold { get; init; } = 5;

    /// <summary>
    /// Sliding window, in seconds, over which <see cref="BreakerThreshold"/>
    /// failures are counted, and the cooldown the breaker stays Open before
    /// admitting a single trial call (HalfOpen). Default 10 seconds.
    /// </summary>
    public int BreakerWindowSeconds { get; init; } = 10;

    /// <summary>
    /// Declared response class when the breaker is Open or a call times out.
    /// Locked to <c>RetryableRefusal</c> in Phase 1.5: §5.2.1 forbids any
    /// shape that could collapse to an implicit allow, and the canonical
    /// 4-way classification model only admits one refusal class for
    /// transient policy unavailability. The field exists so the canonical
    /// configuration block records the choice explicitly per the
    /// no-incidental-defaults rule (R-10).
    /// </summary>
    public string OpenStateBehavior { get; init; } = "RetryableRefusal";
}
