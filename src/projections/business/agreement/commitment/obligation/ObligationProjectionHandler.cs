using Whycespace.Projections.Business.Agreement.Commitment.Obligation.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Obligation;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Agreement.Commitment.Obligation;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.Commitment.Obligation;

public sealed class ObligationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ObligationCreatedEventSchema>,
    IProjectionHandler<ObligationFulfilledEventSchema>,
    IProjectionHandler<ObligationBreachedEventSchema>
{
    private readonly PostgresProjectionStore<ObligationReadModel> _store;

    public ObligationProjectionHandler(PostgresProjectionStore<ObligationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ObligationCreatedEventSchema e   => Project(e.AggregateId, s => ObligationProjectionReducer.Apply(s, e), "ObligationCreatedEvent",   envelope, cancellationToken),
            ObligationFulfilledEventSchema e => Project(e.AggregateId, s => ObligationProjectionReducer.Apply(s, e), "ObligationFulfilledEvent", envelope, cancellationToken),
            ObligationBreachedEventSchema e  => Project(e.AggregateId, s => ObligationProjectionReducer.Apply(s, e), "ObligationBreachedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ObligationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ObligationCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ObligationProjectionReducer.Apply(s, e), "ObligationCreatedEvent", null, ct);

    public Task HandleAsync(ObligationFulfilledEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ObligationProjectionReducer.Apply(s, e), "ObligationFulfilledEvent", null, ct);

    public Task HandleAsync(ObligationBreachedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ObligationProjectionReducer.Apply(s, e), "ObligationBreachedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ObligationReadModel, ObligationReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ObligationReadModel { ObligationId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(
            aggregateId,
            state,
            eventTypeName,
            envelope?.EventId ?? Guid.Empty,
            envelope?.SequenceNumber ?? 0,
            envelope?.CorrelationId ?? Guid.Empty,
            ct);
    }
}
