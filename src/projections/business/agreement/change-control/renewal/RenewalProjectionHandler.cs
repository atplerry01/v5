using Whycespace.Projections.Business.Agreement.ChangeControl.Renewal.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Renewal;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Renewal;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.ChangeControl.Renewal;

public sealed class RenewalProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<RenewalCreatedEventSchema>,
    IProjectionHandler<RenewalRenewedEventSchema>,
    IProjectionHandler<RenewalExpiredEventSchema>
{
    private readonly PostgresProjectionStore<RenewalReadModel> _store;

    public RenewalProjectionHandler(PostgresProjectionStore<RenewalReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            RenewalCreatedEventSchema e => Project(e.AggregateId, s => RenewalProjectionReducer.Apply(s, e), "RenewalCreatedEvent", envelope, cancellationToken),
            RenewalRenewedEventSchema e => Project(e.AggregateId, s => RenewalProjectionReducer.Apply(s, e), "RenewalRenewedEvent", envelope, cancellationToken),
            RenewalExpiredEventSchema e => Project(e.AggregateId, s => RenewalProjectionReducer.Apply(s, e), "RenewalExpiredEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"RenewalProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(RenewalCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RenewalProjectionReducer.Apply(s, e), "RenewalCreatedEvent", null, ct);

    public Task HandleAsync(RenewalRenewedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RenewalProjectionReducer.Apply(s, e), "RenewalRenewedEvent", null, ct);

    public Task HandleAsync(RenewalExpiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RenewalProjectionReducer.Apply(s, e), "RenewalExpiredEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<RenewalReadModel, RenewalReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new RenewalReadModel { RenewalId = aggregateId };
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
