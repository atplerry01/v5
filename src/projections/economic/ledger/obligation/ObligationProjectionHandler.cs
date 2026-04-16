using Whycespace.Projections.Economic.Ledger.Obligation.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Ledger.Obligation;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Obligation;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Ledger.Obligation;

public sealed class ObligationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ObligationCreatedEventSchema>,
    IProjectionHandler<ObligationFulfilledEventSchema>,
    IProjectionHandler<ObligationCancelledEventSchema>
{
    private readonly PostgresProjectionStore<ObligationReadModel> _store;

    public ObligationProjectionHandler(PostgresProjectionStore<ObligationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ObligationCreatedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            ObligationFulfilledEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            ObligationCancelledEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ObligationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(ObligationCreatedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(ObligationFulfilledEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(ObligationCancelledEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(ObligationCreatedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new ObligationReadModel { ObligationId = e.AggregateId };
        state = ObligationProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "ObligationCreatedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(ObligationFulfilledEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new ObligationReadModel { ObligationId = e.AggregateId };
        state = ObligationProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "ObligationFulfilledEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(ObligationCancelledEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new ObligationReadModel { ObligationId = e.AggregateId };
        state = ObligationProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "ObligationCancelledEvent", eventId, eventVersion, correlationId, ct);
    }
}
