namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// Execution context for a command. Carries identity, policy, and correlation metadata.
///
/// T0U HARDENING:
/// - IdentityId, Roles, TrustScore are write-once (set by PolicyMiddleware during WhyceId resolution)
/// - PolicyDecisionAllowed, PolicyDecisionHash, PolicyVersion are write-once (set after WhycePolicy evaluation)
/// - Once set, these values are LOCKED for the remainder of the execution to ensure replay determinism
///
/// phase1.6-S1.4 — REPLAY RESET SEAM:
/// The write-once invariant is preserved at runtime. A controlled, internal-only
/// reset seam (<see cref="EnableReplayMode"/> + <see cref="ResetForReplay"/> +
/// <see cref="DisableReplayMode"/>) exists for a future deterministic replay
/// tool that needs to recompute write-once fields against a historical event
/// stream. The seam is gated by <see cref="IsReplayMode"/> — calling
/// <c>ResetForReplay</c> outside replay mode throws — and is not exposed to
/// any caller outside the shared contracts assembly. When a production caller
/// is eventually added, an architecture invariant
/// (<c>WbsmArchitectureTests.CommandContext_replay_reset_seam_has_no_production_callers</c>)
/// must be updated to whitelist the specific call site. See the locked S1.4
/// execution plan in claude/audits/sweeps/20260408-103326-full-system.md.
/// </summary>
public sealed record CommandContext
{
    public required Guid CorrelationId { get; init; }
    public required Guid CausationId { get; init; }
    public required Guid CommandId { get; init; }
    public required string TenantId { get; init; }
    public required string ActorId { get; init; }
    public required Guid AggregateId { get; init; }
    public required string PolicyId { get; init; }
    public bool RuntimeOrigin { get; set; }

    /// <summary>
    /// Phase 2.5 — system-origin bypass flag. When true, <see cref="EnforcementGuard"/>
    /// allows the command past the restriction gate so workflow-driven
    /// compensation / settlement / recovery commands can complete against
    /// a subject that has become restricted mid-flight. Default false;
    /// set to true ONLY by <see cref="ISystemIntentDispatcher.DispatchSystemAsync"/>.
    /// The flag is init-only (write-once at context construction) — no
    /// setter is exposed so middleware, handlers, and the API surface
    /// cannot promote a user command to system after the fact.
    /// </summary>
    public bool IsSystem { get; init; }

    /// <summary>
    /// Enforcement constraint applied by <c>ExecutionGuardMiddleware</c>
    /// after consulting the active-violation projection for
    /// <see cref="IdentityId"/>. Null when no active non-blocking
    /// violation applies; "Restrict" when a Restrict-action violation
    /// is active. Engine handlers may read this flag to degrade behavior
    /// (e.g. reject limit-increasing commands) without re-querying the
    /// projection. Critical+Block violations short-circuit in the guard
    /// before reaching this field.
    /// </summary>
    public string? EnforcementConstraint { get; set; }

    // Domain routing metadata — used for canonical Kafka topic resolution
    public required string Classification { get; init; }
    public required string Context { get; init; }
    public required string Domain { get; init; }

    // --- T0U Identity (write-once, locked after PolicyMiddleware) ---

    private string? _identityId;
    public string? IdentityId
    {
        get => _identityId;
        set
        {
            if (_identityId is not null)
                throw new InvalidOperationException(
                    "IdentityId is write-once. Already set to '" + _identityId + "'. " +
                    "Trust score, identity, and roles are locked after initial resolution.");
            _identityId = value;
        }
    }

    private string[]? _roles;
    public string[]? Roles
    {
        get => _roles;
        set
        {
            if (_roles is not null)
                throw new InvalidOperationException(
                    "Roles is write-once. Already locked after identity resolution.");
            _roles = value;
        }
    }

    private int? _trustScore;
    public int? TrustScore
    {
        get => _trustScore;
        set
        {
            if (_trustScore is not null)
                throw new InvalidOperationException(
                    "TrustScore is write-once. Already locked to " + _trustScore + ". " +
                    "Trust score is computed ONCE per request and never recomputed during execution.");
            _trustScore = value;
        }
    }

    // --- T0U Policy (write-once, locked after PolicyMiddleware) ---

    private bool? _policyDecisionAllowed;
    public bool? PolicyDecisionAllowed
    {
        get => _policyDecisionAllowed;
        set
        {
            if (_policyDecisionAllowed is not null)
                throw new InvalidOperationException(
                    "PolicyDecisionAllowed is write-once. Already locked after policy evaluation.");
            _policyDecisionAllowed = value;
        }
    }

    private string? _policyDecisionHash;
    public string? PolicyDecisionHash
    {
        get => _policyDecisionHash;
        set
        {
            if (_policyDecisionHash is not null)
                throw new InvalidOperationException(
                    "PolicyDecisionHash is write-once. Already locked after policy evaluation.");
            _policyDecisionHash = value;
        }
    }

    // --- phase1.5-S5.2.4 / HC-7 (DEGRADED-MODE-DEFINITION-01) ---
    // Write-once snapshot of the runtime's Degraded-class posture
    // at the moment dispatch began. Stamped by RuntimeControlPlane
    // BEFORE the middleware pipeline runs so observability /
    // tracing / audit can correlate request behavior with the live
    // degraded reasons. Non-blocking: a Degraded posture does NOT
    // alter dispatch semantics in HC-7 — enforcement is reserved
    // for a later workstream. Locked after first set so middleware
    // cannot rewrite the dispatch-time observation.

    private Whycespace.Shared.Contracts.Infrastructure.Health.RuntimeDegradedMode? _degradedMode;
    public Whycespace.Shared.Contracts.Infrastructure.Health.RuntimeDegradedMode? DegradedMode
    {
        get => _degradedMode;
        set
        {
            if (_degradedMode is not null)
                throw new InvalidOperationException(
                    "DegradedMode is write-once. Already locked.");
            _degradedMode = value;
        }
    }

    // --- phase1.5-S5.2.4 / HC-8 (MAINTENANCE-MODE-ENFORCEMENT-01) ---
    // Write-once tag set by the runtime control plane's enforcement
    // gate when the runtime is Degraded and the command was admitted
    // anyway (i.e. the command does NOT implement
    // IRestrictedDuringDegraded). Strict typed field rather than a
    // free-form metadata bag so the existing CommandContext
    // write-once invariant is preserved. Default false; set true
    // exactly once on the soft-restriction branch of the gate.

    private bool? _isExecutionRestricted;
    public bool IsExecutionRestricted
    {
        get => _isExecutionRestricted ?? false;
        set
        {
            if (_isExecutionRestricted is not null)
                throw new InvalidOperationException(
                    "IsExecutionRestricted is write-once. Already locked.");
            _isExecutionRestricted = value;
        }
    }

    // --- HSID v2.1 (write-once, locked after RuntimeControlPlane prelude) ---

    private string? _hsid;
    /// <summary>
    /// Compact HSID v2.1 correlation id of the form
    /// <c>PPP-LLLL-TTT-TOPOLOGY-SEQ</c>. Stamped exactly once by the
    /// RuntimeControlPlane prelude before the middleware pipeline runs.
    /// Locked after first set to preserve replay determinism.
    /// </summary>
    public string? Hsid
    {
        get => _hsid;
        set
        {
            if (_hsid is not null)
                throw new InvalidOperationException(
                    "Hsid is write-once. Already locked to '" + _hsid + "'.");
            _hsid = value;
        }
    }

    // --- H8b Optimistic Concurrency (write-once, set by RuntimeCommandDispatcher
    //     after the engine loads the aggregate via LoadFromHistory) ---

    private int? _expectedVersion;
    /// <summary>
    /// The aggregate version observed at command-execution time, captured
    /// from <c>AggregateRoot.Version</c> after <c>LoadFromHistory</c>. The
    /// EventFabric forwards this to <see cref="IEventStore.AppendEventsAsync"/>
    /// where it gates an optimistic concurrency check. <c>null</c> (and the
    /// canonical sentinel <c>-1</c>) mean "no check" — used by creation
    /// commands and any caller that has not yet been migrated to assert a
    /// version. Write-once to preserve replay determinism.
    /// </summary>
    public int? ExpectedVersion
    {
        get => _expectedVersion;
        set
        {
            if (_expectedVersion is not null)
                throw new InvalidOperationException(
                    "ExpectedVersion is write-once. Already locked to " + _expectedVersion + ".");
            _expectedVersion = value;
        }
    }

    private string? _policyVersion;
    public string? PolicyVersion
    {
        get => _policyVersion;
        set
        {
            if (_policyVersion is not null)
                throw new InvalidOperationException(
                    "PolicyVersion is write-once. Already locked to '" + _policyVersion + "'. " +
                    "Replay uses the SAME version that was evaluated.");
            _policyVersion = value;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // phase1.6-S1.4 — REPLAY RESET SEAM (internal-only, gated)
    // ─────────────────────────────────────────────────────────────────────
    //
    // Why this exists:
    //   The write-once setters above are correct for runtime execution but
    //   prevent a deterministic replay tool from re-deriving fields against
    //   a historical event stream. Relaxing the write-once guards would
    //   weaken runtime integrity; keeping them strict and adding a tightly
    //   gated reset seam preserves both invariants.
    //
    // Gating rules (LOCKED):
    //   1. ResetForReplay() throws unless EnableReplayMode() has been called
    //      first. EnableReplayMode is the only way to flip the gate.
    //   2. ResetForReplay() resets ALL write-once guards in one shot —
    //      there is no "reset just this field" API. Partial resets are
    //      forbidden by design (a partially-reset context can produce
    //      incoherent state where one field is from the original execution
    //      and another from the replay).
    //   3. The seam is `internal`. Whycespace.Shared exposes it only to
    //      Whycespace.Tests.Unit via InternalsVisibleTo. There is no
    //      production caller today; adding one is a separate gate that
    //      must update the architecture invariant (see WbsmArchitectureTests
    //      .CommandContext_replay_reset_seam_has_no_production_callers).
    //   4. After ResetForReplay returns, the context behaves exactly like
    //      a freshly constructed one with respect to the write-once fields.
    //      The required init-only properties (CorrelationId, CommandId,
    //      etc.) are NOT touched — they were set at construction and are
    //      part of the immutable identity of the context.
    //
    // What is NOT reset:
    //   - The required init-only properties (CorrelationId, CausationId,
    //     CommandId, TenantId, ActorId, AggregateId, PolicyId,
    //     Classification, Context, Domain) — these are construction-time
    //     and have no setter at all.
    //   - RuntimeOrigin — public mutable by design.
    //
    // What IS reset (every write-once guard, in one shot):
    //   IdentityId, Roles, TrustScore,
    //   PolicyDecisionAllowed, PolicyDecisionHash, PolicyVersion,
    //   Hsid, ExpectedVersion.

    private bool _isReplayMode;

    /// <summary>
    /// Returns true if this context is currently in replay mode.
    /// While true, <see cref="ResetForReplay"/> may be called.
    /// Outside replay mode the context behaves identically to its
    /// pre-S1.4 form — every write-once guard is strict.
    /// </summary>
    internal bool IsReplayMode => _isReplayMode;

    /// <summary>
    /// Opens the replay window. Must be called before
    /// <see cref="ResetForReplay"/>. Idempotent — re-enabling already-
    /// enabled mode is a no-op (no exception). The window stays open
    /// until <see cref="DisableReplayMode"/> is called.
    /// </summary>
    internal void EnableReplayMode() => _isReplayMode = true;

    /// <summary>
    /// Closes the replay window. After this call, any further write to
    /// a write-once field that has been set throws as normal. Idempotent.
    /// </summary>
    internal void DisableReplayMode() => _isReplayMode = false;

    /// <summary>
    /// Resets every write-once guard in one shot. ONLY callable while
    /// <see cref="IsReplayMode"/> is true; throws otherwise. Resets the
    /// guards only (not the values' types or the init-only construction-
    /// time fields). After return, the context can accept fresh writes
    /// to the same fields exactly once until either disabled or reset
    /// again.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if called outside replay mode.
    /// </exception>
    internal void ResetForReplay()
    {
        if (!_isReplayMode)
            throw new InvalidOperationException(
                "CommandContext.ResetForReplay() is only valid while in replay mode. " +
                "Call EnableReplayMode() first. Runtime execution must never call this " +
                "method — write-once integrity is preserved by gating the reset behind " +
                "an explicit, scoped replay window. (phase1.6-S1.4)");

        _identityId = null;
        _roles = null;
        _trustScore = null;
        _policyDecisionAllowed = null;
        _policyDecisionHash = null;
        _policyVersion = null;
        _hsid = null;
        _expectedVersion = null;
    }
}
