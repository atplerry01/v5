namespace Whycespace.Shared.Contracts.Infrastructure.Admission;

/// <summary>
/// Tunable behavior for the runtime <c>ChainAnchorService</c>
/// global commit serializer.
///
/// phase1.5-S5.2.2 / KW-1 (CHAIN-ANCHOR-DECLARED-01): closes the
/// governance gap left by §5.2.1 / PC-5 (which made the chain anchor
/// wait/hold time observable but did not externalise the single-permit
/// declaration). KW-1 promotes the seam from <c>DECLARED-OPAQUE</c>
/// (incidental hardcoded <c>SemaphoreSlim(1, 1)</c>) to
/// <c>DECLARED-BOUNDED</c> by binding the permit count to a declared
/// configuration option, satisfying the §5.2.2 acceptance criterion
/// that every concurrency primitive in scope must be declared, even
/// when its value is fixed for structural reasons.
///
/// phase1.5-S5.2.3 / TC-2 (CHAIN-ANCHOR-WAIT-TIMEOUT-01): closes the
/// indefinite-wait gap left by KW-1. The semaphore wait now carries a
/// declared timeout and a declared retry-after hint, mirroring the
/// OutboxOptions / WorkflowOptions / IntakeOptions retryable-refusal
/// pattern. Structural restructuring of the lock — moving the external
/// <c>IChainAnchor.AnchorAsync</c> persist outside the held section,
/// sharding by correlation hash, or replacing the global semaphore
/// with a per-aggregate primitive — remains explicitly deferred.
///
/// Follows the phase1.6-S1.5 OutboxOptions / phase1.5-S5.2.1 OpaOptions
/// / IntakeOptions / phase1.5-S5.2.2 KafkaConsumerOptions / WorkflowOptions
/// precedent — a plain record bound at the composition root from
/// configuration, no <c>IOptions&lt;T&gt;</c> indirection.
/// </summary>
public sealed record ChainAnchorOptions
{
    /// <summary>
    /// Number of concurrent anchor calls permitted in the
    /// <c>ChainAnchorService</c> critical section. Must be at least
    /// 1. Default 1 — the only value the current chain integrity
    /// invariant supports: <c>ChainAnchorService</c> mutates
    /// runtime-owned chain head state (<c>_lastBlockHash</c>,
    /// <c>_lastSequence</c>) inside the critical section, and
    /// concurrent mutators would race on those fields without
    /// the structural restructuring that KW-1 explicitly defers.
    ///
    /// Raising this value above 1 today would corrupt the chain.
    /// The field exists so the single-permit declaration is
    /// canonical, externalised, and audit-visible — not so it can
    /// be tuned. A future structural workstream that moves the
    /// chain head mutation behind a per-correlation primitive
    /// would re-evaluate this default.
    /// </summary>
    public int PermitLimit { get; init; } = 1;

    /// <summary>
    /// phase1.5-S5.2.3 / TC-2 (CHAIN-ANCHOR-WAIT-TIMEOUT-01): maximum
    /// time, in milliseconds, that <c>ChainAnchorService.AnchorAsync</c>
    /// will wait to acquire the global commit serializer before
    /// throwing <c>ChainAnchorWaitTimeoutException</c>. Must be at
    /// least 1. Default 5000 ms — generous enough to absorb a single
    /// in-flight chain-store persist on a healthy node, tight enough
    /// to surface a stuck holder as an explicit retryable refusal
    /// rather than an indefinite request hang.
    /// </summary>
    public int WaitTimeoutMs { get; init; } = 5000;

    /// <summary>
    /// phase1.5-S5.2.3 / TC-2: suggested retry delay, in seconds,
    /// surfaced on the HTTP <c>Retry-After</c> header when the chain
    /// anchor wait times out. Mirrors the OutboxOptions /
    /// WorkflowOptions / IntakeOptions precedent. Default 1 second.
    /// </summary>
    public int RetryAfterSeconds { get; init; } = 1;

    /// <summary>
    /// phase1.5-S5.2.3 / TC-3 (CHAIN-STORE-CT-BREAKER-01): consecutive
    /// chain-store I/O failures (transport, command exception, cancellation
    /// from a non-caller source) tolerated before the chain-store
    /// adapter circuit breaker trips Open. Mirrors the PC-2 OPA breaker.
    /// Must be at least 1. Default 5.
    /// </summary>
    public int BreakerThreshold { get; init; } = 5;

    /// <summary>
    /// phase1.5-S5.2.3 / TC-3: breaker open-window in seconds. Once the
    /// chain-store adapter trips Open, every call refuses immediately
    /// until this window has elapsed. Mirrors the PC-2 OPA breaker.
    /// Doubles as the <c>Retry-After</c> hint surfaced when the breaker
    /// refuses, so a stuck chain-store does not produce a tighter retry
    /// loop than the window itself. Must be at least 1. Default 10.
    /// </summary>
    public int BreakerWindowSeconds { get; init; } = 10;
}
