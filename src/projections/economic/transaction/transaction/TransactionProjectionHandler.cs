using Whycespace.Projections.Economic.Transaction.Transaction.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Transaction.Transaction;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Transaction.Transaction;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Transaction.Transaction;

public sealed class TransactionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<TransactionInitiatedEventSchema>,
    IProjectionHandler<TransactionCommittedEventSchema>,
    IProjectionHandler<TransactionFailedEventSchema>
{
    private readonly PostgresProjectionStore<TransactionReadModel> _store;

    public TransactionProjectionHandler(PostgresProjectionStore<TransactionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            TransactionInitiatedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            TransactionCommittedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            TransactionFailedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"TransactionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(TransactionInitiatedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(TransactionCommittedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(TransactionFailedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(TransactionInitiatedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new TransactionReadModel { TransactionId = e.AggregateId };
        state = TransactionProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "TransactionInitiatedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(TransactionCommittedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new TransactionReadModel { TransactionId = e.AggregateId };
        state = TransactionProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "TransactionCommittedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(TransactionFailedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new TransactionReadModel { TransactionId = e.AggregateId };
        state = TransactionProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "TransactionFailedEvent", eventId, eventVersion, correlationId, ct);
    }
}
