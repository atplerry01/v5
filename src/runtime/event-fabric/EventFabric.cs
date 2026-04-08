using Whyce.Runtime.Deterministic;
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
///   4. OutboxService.Enqueue (Kafka relay — projections consume from Kafka ONLY)
///
/// The fabric contains NO persistence, chain, or messaging logic itself.
/// It is a pure orchestrator that enforces the strict ordering of post-execution stages.
/// Projections are triggered exclusively by Kafka consumers — no direct dispatch.
/// </summary>
public sealed class EventFabric : IEventFabric
{
    private readonly EventStoreService _eventStoreService;
    private readonly ChainAnchorService _chainAnchorService;
    private readonly OutboxService _outboxService;
    private readonly EventSchemaRegistry _schemaRegistry;
    private readonly TopicNameResolver _topicNameResolver;
    private readonly IClock _clock;

    public EventFabric(
        EventStoreService eventStoreService,
        ChainAnchorService chainAnchorService,
        OutboxService outboxService,
        EventSchemaRegistry schemaRegistry,
        TopicNameResolver topicNameResolver,
        IClock clock)
    {
        _eventStoreService = eventStoreService;
        _chainAnchorService = chainAnchorService;
        _outboxService = outboxService;
        _schemaRegistry = schemaRegistry;
        _topicNameResolver = topicNameResolver;
        _clock = clock;
    }

    public Task ProcessAsync(IReadOnlyList<object> domainEvents, CommandContext context) =>
        ProcessInternalAsync(
            domainEvents,
            context,
            aggregateIdOverride: null,
            classificationOverride: null,
            contextOverride: null,
            domainOverride: null);

    /// <summary>
    /// Processes an audit emission with explicit routing overrides. Used by the
    /// runtime control plane for policy decision events that must be persisted
    /// to a dedicated stream and published to a dedicated topic, isolated from
    /// the command's domain aggregate stream.
    /// </summary>
    public Task ProcessAuditAsync(AuditEmission audit, CommandContext context) =>
        ProcessInternalAsync(
            audit.Events,
            context,
            aggregateIdOverride: audit.AggregateId,
            classificationOverride: audit.Classification,
            contextOverride: audit.Context,
            domainOverride: audit.Domain);

    private async Task ProcessInternalAsync(
        IReadOnlyList<object> domainEvents,
        CommandContext context,
        Guid? aggregateIdOverride,
        string? classificationOverride,
        string? contextOverride,
        string? domainOverride)
    {
        if (domainEvents.Count == 0) return;

        var aggregateId = aggregateIdOverride ?? context.AggregateId;
        var classification = classificationOverride ?? context.Classification;
        var ctxName = contextOverride ?? context.Context;
        var domainName = domainOverride ?? context.Domain;

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
                AggregateId = aggregateId,
                CorrelationId = context.CorrelationId,
                EventType = eventTypeName,
                EventName = schema.EventName,
                EventVersion = schema.Version,
                SchemaHash = schema.SchemaHash,
                Payload = _schemaRegistry.MapPayload(eventTypeName, domainEvent),
                ExecutionHash = executionHash,
                PolicyHash = policyHash,
                Timestamp = _clock.UtcNow,
                SequenceNumber = i,
                Classification = classification,
                Context = ctxName,
                Domain = domainName
            });
        }

        // Step 1b: Resolve canonical Kafka topic from envelope routing metadata
        var topic = _topicNameResolver.Resolve(envelopes[0], "events");

        // Step 2: Persist to EventStore (source of truth)
        // phase1-gate-H8b: forward the dispatcher-captured ExpectedVersion as
        // the optimistic concurrency assertion. null → -1 sentinel (no check).
        await _eventStoreService.AppendAsync(aggregateId, domainEvents, context.ExpectedVersion ?? -1);

        // Step 3: Anchor to WhyceChain (MUST happen AFTER persistence)
        await _chainAnchorService.AnchorAsync(context.CorrelationId, domainEvents, policyHash);

        // Step 4: Enqueue to Outbox with resolved topic (Kafka relay → consumer → projection)
        await _outboxService.EnqueueAsync(context.CorrelationId, domainEvents, topic);
    }
}
