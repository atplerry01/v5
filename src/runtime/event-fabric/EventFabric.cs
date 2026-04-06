using Whyce.Runtime.Deterministic;
using Whyce.Runtime.Projection;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Runtime.EventFabric;

/// <summary>
/// Event Fabric — ORCHESTRATOR ONLY.
///
/// Accepts domain events and orchestrates the canonical post-execution sequence
/// by delegating to specialized services:
///
///   1. Wrap events into deterministic EventEnvelopes (with versioning)
///   2. EventStoreService.Append (persist — source of truth)
///   3. ChainAnchorService.Anchor (immutable audit trail)
///   4. ProjectionDispatcher.Dispatch (read model updates)
///   5. OutboxService.Enqueue (bridge to external systems)
///
/// The fabric contains NO persistence, chain, or messaging logic itself.
/// It is a pure orchestrator that enforces the strict ordering of post-execution stages.
/// </summary>
public sealed class EventFabric : IEventFabric
{
    private readonly EventStoreService _eventStoreService;
    private readonly ChainAnchorService _chainAnchorService;
    private readonly IProjectionDispatcher _projectionDispatcher;
    private readonly OutboxService _outboxService;
    private readonly EventSchemaRegistry _schemaRegistry;
    private readonly IClock _clock;

    public EventFabric(
        EventStoreService eventStoreService,
        ChainAnchorService chainAnchorService,
        IProjectionDispatcher projectionDispatcher,
        OutboxService outboxService,
        EventSchemaRegistry schemaRegistry,
        IClock clock)
    {
        _eventStoreService = eventStoreService;
        _chainAnchorService = chainAnchorService;
        _projectionDispatcher = projectionDispatcher;
        _outboxService = outboxService;
        _schemaRegistry = schemaRegistry;
        _clock = clock;
    }

    public async Task ProcessAsync(IReadOnlyList<object> domainEvents, CommandContext context)
    {
        if (domainEvents.Count == 0) return;

        var policyHash = context.PolicyDecisionHash ?? string.Empty;
        var executionHash = ExecutionHash.Compute(context, domainEvents);

        // Step 1: Wrap domain events into deterministic envelopes with versioning
        var envelopes = new List<EventEnvelope>(domainEvents.Count);
        for (var i = 0; i < domainEvents.Count; i++)
        {
            var domainEvent = domainEvents[i];
            var eventTypeName = domainEvent.GetType().Name;
            var schema = _schemaRegistry.Resolve(eventTypeName);

            envelopes.Add(new EventEnvelope
            {
                EventId = EventEnvelope.GenerateDeterministicId(
                    context.CorrelationId, eventTypeName, i),
                AggregateId = context.AggregateId,
                CorrelationId = context.CorrelationId,
                EventType = eventTypeName,
                EventName = schema.EventName,
                EventVersion = schema.Version,
                SchemaHash = schema.SchemaHash,
                Payload = domainEvent,
                ExecutionHash = executionHash,
                PolicyHash = policyHash,
                Timestamp = _clock.UtcNow,
                SequenceNumber = i
            });
        }

        // Step 2: Persist to EventStore (source of truth)
        await _eventStoreService.AppendAsync(context.AggregateId, domainEvents);

        // Step 3: Anchor to WhyceChain (MUST happen AFTER persistence)
        await _chainAnchorService.AnchorAsync(context.CorrelationId, domainEvents, policyHash);

        // Step 4: Dispatch to Projection system
        await _projectionDispatcher.DispatchAsync(envelopes);

        // Step 5: Enqueue to Outbox (bridge to external systems)
        await _outboxService.EnqueueAsync(context.CorrelationId, domainEvents);
    }
}
