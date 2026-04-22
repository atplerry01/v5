using Whycespace.Projections.Platform.Envelope.MessageEnvelope.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Platform.Envelope.MessageEnvelope;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Platform.Envelope.MessageEnvelope;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Platform.Envelope.MessageEnvelope;

public sealed class MessageEnvelopeProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<MessageEnvelopeCreatedEventSchema>,
    IProjectionHandler<MessageEnvelopeDispatchedEventSchema>,
    IProjectionHandler<MessageEnvelopeAcknowledgedEventSchema>,
    IProjectionHandler<MessageEnvelopeRejectedEventSchema>
{
    private readonly PostgresProjectionStore<MessageEnvelopeReadModel> _store;

    public MessageEnvelopeProjectionHandler(PostgresProjectionStore<MessageEnvelopeReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            MessageEnvelopeCreatedEventSchema e => Project(e.AggregateId, s => MessageEnvelopeProjectionReducer.Apply(s, e, envelope.Timestamp), "MessageEnvelopeCreatedEvent", envelope, cancellationToken),
            MessageEnvelopeDispatchedEventSchema e => Project(e.AggregateId, s => MessageEnvelopeProjectionReducer.Apply(s, e, envelope.Timestamp), "MessageEnvelopeDispatchedEvent", envelope, cancellationToken),
            MessageEnvelopeAcknowledgedEventSchema e => Project(e.AggregateId, s => MessageEnvelopeProjectionReducer.Apply(s, e, envelope.Timestamp), "MessageEnvelopeAcknowledgedEvent", envelope, cancellationToken),
            MessageEnvelopeRejectedEventSchema e => Project(e.AggregateId, s => MessageEnvelopeProjectionReducer.Apply(s, e, envelope.Timestamp), "MessageEnvelopeRejectedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"MessageEnvelopeProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(MessageEnvelopeCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MessageEnvelopeProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "MessageEnvelopeCreatedEvent", null, ct);
    public Task HandleAsync(MessageEnvelopeDispatchedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MessageEnvelopeProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "MessageEnvelopeDispatchedEvent", null, ct);
    public Task HandleAsync(MessageEnvelopeAcknowledgedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MessageEnvelopeProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "MessageEnvelopeAcknowledgedEvent", null, ct);
    public Task HandleAsync(MessageEnvelopeRejectedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MessageEnvelopeProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "MessageEnvelopeRejectedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<MessageEnvelopeReadModel, MessageEnvelopeReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new MessageEnvelopeReadModel { EnvelopeId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
