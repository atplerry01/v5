using Whyce.Engines.T0U.Determinism.Sequence;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.Middleware;
using Whyce.Runtime.Topology;
using Whyce.Shared.Contracts.Infrastructure.Health;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Determinism;

namespace Whyce.Runtime.ControlPlane;

/// <summary>
/// Runtime Control Plane — SINGLE ENTRY POINT for all command execution.
/// ControlPlane → EventFabric is SINGLE and NON-BYPASSABLE.
///
/// T0U HARDENING — Defense-in-depth policy enforcement:
/// After middleware pipeline completes, the control plane VERIFIES that
/// PolicyDecisionAllowed == true before dispatching and before EventFabric.
/// This guards against middleware misconfiguration where PolicyMiddleware is missing.
///
/// HSID v2.1 HARDENING (H2/H4):
/// The deterministic ID engine, sequence resolver, and topology resolver are
/// MANDATORY. There is no fallback path — missing wiring fails the
/// constructor. Topology is resolved authoritatively when the command
/// implements <see cref="IHsidCommand"/>; otherwise it is derived
/// deterministically from classification|context|domain (still stable, but
/// not authoritative).
///
/// Execution Model:
///   ControlPlane (HSID prelude — out-of-band)
///     → Middleware Pipeline (locked order)
///     → Policy Guard (defense-in-depth)
///     → Dispatcher → Engine
///     → EventFabric (orchestrator: persist → chain → projection → outbox)
///     → Return Response
/// </summary>
public sealed class RuntimeControlPlane : IRuntimeControlPlane
{
    private readonly IReadOnlyList<IMiddleware> _middlewares;
    private readonly ICommandDispatcher _dispatcher;
    private readonly IEventFabric _eventFabric;
    private readonly IDeterministicIdEngine _hsidEngine;
    private readonly ISequenceResolver _hsidSequence;
    private readonly ITopologyResolver _topologyResolver;
    private readonly IRuntimeStateAggregator _runtimeStateAggregator;
    private readonly IRuntimeMaintenanceModeProvider _maintenanceProvider;
    private readonly IExecutionLockProvider _executionLockProvider;

    // phase1.5-S5.2.5 / MI-1 (DISTRIBUTED-EXECUTION-SAFETY-01):
    // canonical execution-lock TTL. 30 seconds matches the
    // declared host shutdown drain ceiling (TC-9
    // Host:ShutdownTimeoutSeconds default) so a request that
    // outlives its lease is also a request that has outlived the
    // declared drain envelope. MI-1 does NOT introduce lease
    // renewal — that is reserved for a future workstream.
    private static readonly TimeSpan ExecutionLockTtl = TimeSpan.FromSeconds(30);

    // phase1.5-S5.2.4 / HC-7 (DEGRADED-MODE-DEFINITION-01): the
    // control plane consults the canonical IRuntimeStateAggregator
    // contract (NOT the host-side concrete) so the runtime → host
    // dependency edge forbidden by DG-R5-EXCEPT-01 is preserved.
    // The contract lives in Whyce.Shared.Contracts which Whyce.Runtime
    // already references; the host-side concrete continues to be
    // the only implementer.
    public RuntimeControlPlane(
        IReadOnlyList<IMiddleware> middlewares,
        ICommandDispatcher dispatcher,
        IEventFabric eventFabric,
        IDeterministicIdEngine hsidEngine,
        ISequenceResolver hsidSequence,
        ITopologyResolver topologyResolver,
        IRuntimeStateAggregator runtimeStateAggregator,
        IRuntimeMaintenanceModeProvider maintenanceProvider,
        IExecutionLockProvider executionLockProvider)
    {
        _middlewares = middlewares ?? throw new ArgumentNullException(nameof(middlewares));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _eventFabric = eventFabric ?? throw new ArgumentNullException(nameof(eventFabric));
        _hsidEngine = hsidEngine ?? throw new InvalidOperationException(
            "HSID enforcement failure: IDeterministicIdEngine not configured.");
        _hsidSequence = hsidSequence ?? throw new InvalidOperationException(
            "HSID enforcement failure: ISequenceResolver not configured.");
        _topologyResolver = topologyResolver ?? throw new InvalidOperationException(
            "HSID enforcement failure: ITopologyResolver not configured.");
        _runtimeStateAggregator = runtimeStateAggregator
            ?? throw new ArgumentNullException(nameof(runtimeStateAggregator));
        _maintenanceProvider = maintenanceProvider
            ?? throw new ArgumentNullException(nameof(maintenanceProvider));
        _executionLockProvider = executionLockProvider
            ?? throw new ArgumentNullException(nameof(executionLockProvider));
    }

    public async Task<CommandResult> ExecuteAsync(object command, CommandContext context, CancellationToken cancellationToken = default)
    {
        // phase1.5-S5.2.5 / MI-1 (DISTRIBUTED-EXECUTION-SAFETY-01):
        // distributed execution lock keyed by CommandId. Acquired
        // BEFORE the HSID prelude / pipeline so two instances
        // attempting the same command observe deterministic
        // ownership. Released in the finally block so a thrown
        // exception still surrenders the lease. The lock is
        // owner-safe by construction (Lua CAS in
        // RedisExecutionLockProvider) so a stale process whose
        // lease has expired cannot accidentally unlock a key that
        // has since been re-acquired by another owner.
        var lockKey = $"whyce:execution-lock:{context.CommandId:N}";
        var acquired = await _executionLockProvider.TryAcquireAsync(
            lockKey, ExecutionLockTtl, cancellationToken);
        if (!acquired)
        {
            // phase1.5-S5.2.4 / HC-9 (REDIS-HEALTH-01):
            // deterministic failure family. The lock provider is
            // contractually exception-free; a false return means
            // either (a) another instance currently holds the
            // lock, or (b) Redis itself is unavailable / failed
            // the acquire round-trip. We cannot distinguish the
            // two from this seam without coupling the contract to
            // a specific store, so the canonical refusal is the
            // broader "execution_lock_unavailable" family. The
            // host-shutdown / client-disconnect cancellation
            // branch is reported separately so callers can
            // distinguish a graceful drain from an infrastructure
            // outage.
            if (cancellationToken.IsCancellationRequested)
                return CommandResult.Failure("execution_cancelled");
            return CommandResult.Failure("execution_lock_unavailable");
        }

        try
        {
        // HSID v2.1 PRELUDE — out-of-band of the locked 8-middleware pipeline.
        // Stamps a compact deterministic correlation ID before any middleware
        // runs, so downstream components (tracing, audit, chain anchor) can
        // reference it.
        await StampHsidAsync(command, context);

        // phase1.5-S5.2.4 / HC-7 (DEGRADED-MODE-DEFINITION-01):
        // stamp the dispatch-time degraded posture onto the
        // CommandContext BEFORE the middleware pipeline runs so
        // tracing, metrics, audit, and any downstream observer can
        // correlate request behavior with the live runtime state.
        // GetDegradedMode() is dispatch-cheap by contract (no
        // IHealthCheck fan-out). HC-7 is non-blocking — a degraded
        // posture does not alter dispatch semantics; the tag exists
        // for awareness only. Stamped exactly once (write-once on
        // CommandContext); a defensive null-check tolerates the
        // unlikely case of a pre-stamped context (e.g. test
        // harnesses).
        if (context.DegradedMode is null)
        {
            context.DegradedMode = _runtimeStateAggregator.GetDegradedMode();
        }

        // phase1.5-S5.2.4 / HC-8 (MAINTENANCE-MODE-ENFORCEMENT-01):
        // enforcement gate. Evaluated BEFORE the middleware
        // pipeline so a maintenance hard-block or a
        // restricted-during-degraded hard-block refuses the command
        // without invoking validation, policy, idempotency, or any
        // engine work. The decision is computed by the pure
        // RuntimeEnforcementGate so the rule has a single owner
        // and is unit-testable in isolation. Returned failures use
        // CommandResult.Failure with a deterministic low-cardinality
        // reason — never throws.
        var maintenance = _maintenanceProvider.Get();
        var degraded = context.DegradedMode ?? RuntimeDegradedMode.None;
        var decision = RuntimeEnforcementGate.Evaluate(maintenance, degraded, command);
        switch (decision.Outcome)
        {
            case RuntimeEnforcementOutcome.BlockMaintenance:
                return CommandResult.Failure(decision.Reason!);
            case RuntimeEnforcementOutcome.BlockRestricted:
                return CommandResult.Failure(decision.Reason!);
            case RuntimeEnforcementOutcome.ProceedRestricted:
                // Soft tag — the dispatch proceeds normally; the
                // tag exists so middleware / observability / audit
                // can correlate request behavior with the live
                // degraded posture without re-evaluating the rule.
                if (!context.IsExecutionRestricted)
                    context.IsExecutionRestricted = true;
                break;
            case RuntimeEnforcementOutcome.Proceed:
            default:
                break;
        }

        // phase1.5-S5.2.3 / TC-1 (DISPATCHER-CT-CONTRACT-01): the
        // pipeline closure shape is now Func<CancellationToken,
        // Task<CommandResult>> so each middleware forwards the
        // request/host-shutdown token to the next link via
        // next(ct). The token reaches DispatchWithPolicyGuard at
        // the terminal step and is forwarded into
        // ICommandDispatcher.DispatchAsync.
        Func<CancellationToken, Task<CommandResult>> pipeline =
            ct => DispatchWithPolicyGuard(command, context, ct);

        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var next = pipeline;
            pipeline = ct => middleware.ExecuteAsync(context, command, next, ct);
        }

        var result = await pipeline(cancellationToken);

        // Event emission boundary — single, non-bypassable fabric invocation.
        //
        // Order:
        //  1. AUDIT emission (if present): policy decision events go FIRST,
        //     to a dedicated stream + topic via routing overrides. Persisted
        //     even when IsSuccess=false (deny path), but still requires a valid
        //     PolicyDecisionHash so the denial is chain-anchored to a real
        //     engine decision rather than a bypass.
        //  2. DOMAIN emission (success only): the command's domain events,
        //     persisted under the command's aggregate. Requires
        //     PolicyDecisionAllowed == true (chain-integrity guard).
        if (result.AuditEmission is { Events.Count: > 0 } audit)
        {
            if (string.IsNullOrEmpty(context.PolicyDecisionHash))
            {
                return CommandResult.Failure(
                    "WHYCEPOLICY HARD STOP: Audit emission requires PolicyDecisionHash. " +
                    "Chain anchoring requires a valid decision hash.");
            }

            await _eventFabric.ProcessAuditAsync(audit, context, cancellationToken);
        }

        if (result.IsSuccess && result.EventsRequirePersistence && result.EmittedEvents.Count > 0)
        {
            if (context.PolicyDecisionAllowed != true)
            {
                return CommandResult.Failure(
                    "WHYCEPOLICY HARD STOP: Events cannot be persisted without policy approval. " +
                    "PolicyDecisionAllowed is not true. Chain integrity requires policy evaluation.");
            }

            if (string.IsNullOrEmpty(context.PolicyDecisionHash))
            {
                return CommandResult.Failure(
                    "WHYCEPOLICY HARD STOP: Events cannot be persisted without PolicyDecisionHash. " +
                    "Chain anchoring requires a valid decision hash.");
            }

            await _eventFabric.ProcessAsync(result.EmittedEvents, context, cancellationToken);
        }

        return result;
        }
        finally
        {
            // phase1.5-S5.2.5 / MI-1: ALWAYS release the lock, even
            // on a thrown exception. ReleaseAsync is owner-safe and
            // never throws on a missing key, so this is unconditionally
            // safe to call. Exceptions from ExecuteAsync continue to
            // propagate to the caller — the lock release is purely
            // additive.
            await _executionLockProvider.ReleaseAsync(lockKey);
        }
    }

    /// <summary>
    /// Defense-in-depth: Verify policy was evaluated BEFORE dispatching to engine.
    /// Guards against middleware misconfiguration (missing PolicyMiddleware).
    /// </summary>
    private Task<CommandResult> DispatchWithPolicyGuard(object command, CommandContext context, CancellationToken cancellationToken)
    {
        if (context.PolicyDecisionAllowed != true)
        {
            return Task.FromResult(CommandResult.Failure(
                "WHYCEPOLICY HARD STOP: Command dispatch blocked. PolicyDecisionAllowed is not true. " +
                "No engine execution without explicit policy approval. No bypass allowed."));
        }

        if (string.IsNullOrEmpty(context.IdentityId))
        {
            return Task.FromResult(CommandResult.Failure(
                "WHYCEID HARD STOP: Command dispatch blocked. IdentityId is not set. " +
                "No engine execution without identity resolution. No bypass allowed."));
        }

        return _dispatcher.DispatchAsync(command, context, cancellationToken);
    }

    /// <summary>
    /// HSID v2.1 prelude. Derives a deterministic compact correlation id from
    /// the command context coordinates and stamps it onto
    /// <see cref="CommandContext.Hsid"/>.
    ///
    /// Topology source:
    /// <list type="bullet">
    ///   <item>If the command implements <see cref="IHsidCommand"/>, topology
    ///         is resolved AUTHORITATIVELY via <see cref="ITopologyResolver"/>
    ///         using the command's <c>SpvId</c>.</item>
    ///   <item>Otherwise, topology is DERIVED deterministically from
    ///         classification|context|domain (still replay-stable, but not
    ///         authoritative — the deterministic-id audit A14 covers this
    ///         fallback).</item>
    /// </list>
    ///
    /// No clock, no RNG. Sequence comes from <see cref="ISequenceResolver"/>
    /// keyed on "{topology}:{seed}".
    /// </summary>
    private async Task StampHsidAsync(object command, CommandContext context)
    {
        if (context.Hsid is not null) return;

        var seed = $"{context.CorrelationId:N}:{context.AggregateId:N}";
        var topology = command is IHsidCommand hsidCommand
            ? _topologyResolver.Resolve(hsidCommand.SpvId)
            : DeriveTopology(context);
        var location = DeriveLocation(context);
        var scope = $"{topology}:{seed}";

        var sequence = await _hsidSequence.NextAsync(scope);
        context.Hsid = _hsidEngine.Generate(IdPrefix.CMD, location, topology, seed, sequence);

        if (!_hsidEngine.IsValid(context.Hsid))
            throw new InvalidOperationException(
                "HSID v2.1: generated id failed structural validation: '" + context.Hsid + "'.");
    }

    private static TopologyCode DeriveTopology(CommandContext context)
    {
        var triple = $"{context.Classification}|{context.Context}|{context.Domain}";
        var hex = System.Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(triple)));
        return new TopologyCode(
            Cluster: hex[..3],
            SubCluster: hex[3..6],
            Spv: hex[6..12]);
    }

    private static LocationCode DeriveLocation(CommandContext context)
    {
        var hex = System.Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(context.TenantId)));
        return new LocationCode(hex[..4]);
    }
}
