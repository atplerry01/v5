using Whyce.Runtime.EventFabric;
using Whyce.Runtime.Middleware;
using Whyce.Shared.Contracts.Runtime;

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
/// Execution Model:
///   ControlPlane
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

    public RuntimeControlPlane(
        IReadOnlyList<IMiddleware> middlewares,
        ICommandDispatcher dispatcher,
        IEventFabric eventFabric)
    {
        _middlewares = middlewares;
        _dispatcher = dispatcher;
        _eventFabric = eventFabric;
    }

    public async Task<CommandResult> ExecuteAsync(object command, CommandContext context)
    {
        // Build middleware pipeline wrapping the dispatch + policy guard
        Func<Task<CommandResult>> pipeline = () => DispatchWithPolicyGuard(command, context);

        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var next = pipeline;
            pipeline = () => middleware.ExecuteAsync(context, command, next);
        }

        var result = await pipeline();

        // Event emission boundary — single, non-bypassable fabric invocation
        if (result.IsSuccess && result.EventsRequirePersistence && result.EmittedEvents.Count > 0)
        {
            // HARD STOP: Verify policy was evaluated before persisting events
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
}
