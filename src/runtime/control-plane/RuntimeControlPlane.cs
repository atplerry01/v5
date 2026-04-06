using Whyce.Runtime.Pipeline;
using Whyce.Shared.Contracts.Infrastructure.Chain;
using Whyce.Shared.Contracts.Infrastructure.Messaging;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.ControlPlane;

public sealed class RuntimeControlPlane : IRuntimeControlPlane
{
    private readonly IReadOnlyList<IMiddleware> _middlewares;
    private readonly ICommandDispatcher _dispatcher;
    private readonly IEventStore _eventStore;
    private readonly IChainAnchor _chainAnchor;
    private readonly IOutbox _outbox;

    public RuntimeControlPlane(
        IEnumerable<IMiddleware> middlewares,
        ICommandDispatcher dispatcher,
        IEventStore eventStore,
        IChainAnchor chainAnchor,
        IOutbox outbox)
    {
        _middlewares = middlewares.ToList();
        _dispatcher = dispatcher;
        _eventStore = eventStore;
        _chainAnchor = chainAnchor;
        _outbox = outbox;
    }

    public async Task<CommandResult> ExecuteAsync(object command, CommandContext context)
    {
        // Build middleware pipeline ending with dispatcher
        Func<Task<CommandResult>> pipeline = () => _dispatcher.DispatchAsync(command, context);

        // Wrap in reverse order so first middleware runs first
        // Order: ContextGuard → Policy (WhyceID + WhycePolicy) → Idempotency → Execution
        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var next = pipeline;
            pipeline = () => middleware.ExecuteAsync(context, command, next);
        }

        var result = await pipeline();

        // Persist → Chain → Outbox (strict order per E12)
        // Only persist if events require it (engine commands = yes, workflow sub-commands already persisted)
        if (result.IsSuccess && result.EventsRequirePersistence && result.EmittedEvents.Count > 0)
        {
            await _eventStore.AppendEventsAsync(context.AggregateId, result.EmittedEvents, expectedVersion: -1);
            await _chainAnchor.AnchorAsync(context.CorrelationId, result.EmittedEvents, context.PolicyDecisionHash ?? string.Empty);
            await _outbox.EnqueueAsync(context.CorrelationId, result.EmittedEvents);
        }

        return result;
    }
}
