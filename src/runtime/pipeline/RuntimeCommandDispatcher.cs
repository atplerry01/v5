using Microsoft.Extensions.DependencyInjection;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Infrastructure.Chain;
using Whyce.Shared.Contracts.Infrastructure.Messaging;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Contracts.Runtime;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whyce.Runtime.Pipeline;

public sealed class RuntimeCommandDispatcher : ICommandDispatcher
{
    private readonly IReadOnlyList<IMiddleware> _middlewares;
    private readonly IEngineRegistry _engineRegistry;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventStore _eventStore;
    private readonly IChainAnchor _chainAnchor;
    private readonly IOutbox _outbox;

    public RuntimeCommandDispatcher(
        IEnumerable<IMiddleware> middlewares,
        IEngineRegistry engineRegistry,
        IServiceProvider serviceProvider,
        IEventStore eventStore,
        IChainAnchor chainAnchor,
        IOutbox outbox)
    {
        _middlewares = middlewares.ToList();
        _engineRegistry = engineRegistry;
        _serviceProvider = serviceProvider;
        _eventStore = eventStore;
        _chainAnchor = chainAnchor;
        _outbox = outbox;
    }

    public async Task<CommandResult> DispatchAsync(object command, CommandContext context)
    {
        // Build middleware pipeline ending with engine execution
        Func<Task<CommandResult>> pipeline = () => ExecuteEngineAsync(command, context);

        // Wrap in reverse order so first middleware runs first
        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var next = pipeline;
            pipeline = () => middleware.ExecuteAsync(context, command, next);
        }

        return await pipeline();
    }

    private async Task<CommandResult> ExecuteEngineAsync(object command, CommandContext context)
    {
        var engineType = _engineRegistry.ResolveEngine(command.GetType());
        if (engineType is null)
        {
            return CommandResult.Failure($"No engine registered for {command.GetType().Name}.");
        }

        var engine = (IEngine)_serviceProvider.GetRequiredService(engineType);

        var engineContext = new EngineContext(
            command,
            context.AggregateId,
            async (type, aggregateId) =>
            {
                var aggregate = (AggregateRoot)Activator.CreateInstance(type, nonPublic: true)!;
                var events = await _eventStore.LoadEventsAsync(aggregateId);
                aggregate.LoadFromHistory(events);
                return aggregate;
            });

        await engine.ExecuteAsync(engineContext);

        var emittedEvents = engineContext.EmittedEvents;
        if (emittedEvents.Count == 0)
        {
            return CommandResult.Success([]);
        }

        // Persist → Chain → Outbox (Kafka) — strict order per E12
        await _eventStore.AppendEventsAsync(context.AggregateId, emittedEvents, expectedVersion: -1);
        await _chainAnchor.AnchorAsync(context.CorrelationId, emittedEvents, context.PolicyDecisionHash ?? string.Empty);
        await _outbox.EnqueueAsync(context.CorrelationId, emittedEvents);

        return CommandResult.Success(emittedEvents);
    }
}
