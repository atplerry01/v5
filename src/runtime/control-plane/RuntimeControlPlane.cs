using Whyce.Engines.T0U.Determinism.Sequence;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.Middleware;
using Whyce.Runtime.Topology;
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

    public RuntimeControlPlane(
        IReadOnlyList<IMiddleware> middlewares,
        ICommandDispatcher dispatcher,
        IEventFabric eventFabric,
        IDeterministicIdEngine hsidEngine,
        ISequenceResolver hsidSequence,
        ITopologyResolver topologyResolver)
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
    }

    public async Task<CommandResult> ExecuteAsync(object command, CommandContext context)
    {
        // HSID v2.1 PRELUDE — out-of-band of the locked 8-middleware pipeline.
        // Stamps a compact deterministic correlation ID before any middleware
        // runs, so downstream components (tracing, audit, chain anchor) can
        // reference it.
        await StampHsidAsync(command, context);

        // Build middleware pipeline wrapping the dispatch + policy guard
        Func<Task<CommandResult>> pipeline = () => DispatchWithPolicyGuard(command, context);

        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var next = pipeline;
            pipeline = () => middleware.ExecuteAsync(context, command, next);
        }

        var result = await pipeline();

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

            await _eventFabric.ProcessAuditAsync(audit, context);
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

            await _eventFabric.ProcessAsync(result.EmittedEvents, context);
        }

        return result;
    }

    /// <summary>
    /// Defense-in-depth: Verify policy was evaluated BEFORE dispatching to engine.
    /// Guards against middleware misconfiguration (missing PolicyMiddleware).
    /// </summary>
    private Task<CommandResult> DispatchWithPolicyGuard(object command, CommandContext context)
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

        return _dispatcher.DispatchAsync(command, context);
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
