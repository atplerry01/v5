using Whycespace.Projections.Economic.Revenue.Distribution.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Revenue.Distribution;

public sealed class DistributionCreatedProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<DistributionCreatedEventSchema>,
    IProjectionHandler<DistributionCompensationRequestedEventSchema>,
    IProjectionHandler<DistributionCompensatedEventSchema>
{
    private readonly PostgresProjectionStore<DistributionReadModel> _store;

    public DistributionCreatedProjectionHandler(PostgresProjectionStore<DistributionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            DistributionCreatedEventSchema e                 => Project(e.AggregateId, s => DistributionProjectionReducer.Apply(s, e), "DistributionCreatedEvent", envelope, cancellationToken),
            DistributionCompensationRequestedEventSchema e   => Project(e.AggregateId, s => DistributionProjectionReducer.Apply(s, e), "DistributionCompensationRequestedEvent", envelope, cancellationToken),
            DistributionCompensatedEventSchema e             => Project(e.AggregateId, s => DistributionProjectionReducer.Apply(s, e), "DistributionCompensatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"DistributionCreatedProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(DistributionCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DistributionProjectionReducer.Apply(s, e), "DistributionCreatedEvent", null, ct);

    public Task HandleAsync(DistributionCompensationRequestedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DistributionProjectionReducer.Apply(s, e), "DistributionCompensationRequestedEvent", null, ct);

    public Task HandleAsync(DistributionCompensatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DistributionProjectionReducer.Apply(s, e), "DistributionCompensatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<DistributionReadModel, DistributionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new DistributionReadModel { DistributionId = aggregateId };
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
