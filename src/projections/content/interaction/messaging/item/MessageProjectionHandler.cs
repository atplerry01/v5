using Whycespace.Projections.Content.Interaction.Messaging.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Interaction.Messaging;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Interaction.Messaging;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Interaction.Messaging.Item;

public sealed class MessageProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<MessageSentEventSchema>,
    IProjectionHandler<MessageDeliveredEventSchema>,
    IProjectionHandler<MessageReadEventSchema>,
    IProjectionHandler<MessageEditedEventSchema>,
    IProjectionHandler<MessageRetractedEventSchema>
{
    private readonly PostgresProjectionStore<MessageReadModel> _store;

    public MessageProjectionHandler(PostgresProjectionStore<MessageReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            MessageSentEventSchema e => Project(e.AggregateId, s => MessageProjectionReducer.Apply(s, e), "MessageSentEvent", envelope, cancellationToken),
            MessageDeliveredEventSchema e => Project(e.AggregateId, s => MessageProjectionReducer.Apply(s, e), "MessageDeliveredEvent", envelope, cancellationToken),
            MessageReadEventSchema e => Project(e.AggregateId, s => MessageProjectionReducer.Apply(s, e), "MessageReadEvent", envelope, cancellationToken),
            MessageEditedEventSchema e => Project(e.AggregateId, s => MessageProjectionReducer.Apply(s, e), "MessageEditedEvent", envelope, cancellationToken),
            MessageRetractedEventSchema e => Project(e.AggregateId, s => MessageProjectionReducer.Apply(s, e), "MessageRetractedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"MessageProjectionHandler received unmatched event type: {envelope.Payload.GetType().Name}. " +
                $"EventId={envelope.EventId}, EventType={envelope.EventType}.")
        };
    }

    public Task HandleAsync(MessageSentEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MessageProjectionReducer.Apply(s, e), "MessageSentEvent", null, ct);
    public Task HandleAsync(MessageDeliveredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MessageProjectionReducer.Apply(s, e), "MessageDeliveredEvent", null, ct);
    public Task HandleAsync(MessageReadEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MessageProjectionReducer.Apply(s, e), "MessageReadEvent", null, ct);
    public Task HandleAsync(MessageEditedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MessageProjectionReducer.Apply(s, e), "MessageEditedEvent", null, ct);
    public Task HandleAsync(MessageRetractedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MessageProjectionReducer.Apply(s, e), "MessageRetractedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<MessageReadModel, MessageReadModel> reduce,
        string eventType,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new MessageReadModel { Id = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(
            aggregateId,
            state,
            eventType,
            envelope?.EventId ?? Guid.Empty,
            envelope?.SequenceNumber ?? 0,
            envelope?.CorrelationId ?? Guid.Empty,
            ct);
    }
}
