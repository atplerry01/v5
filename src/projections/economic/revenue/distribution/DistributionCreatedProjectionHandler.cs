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
    IProjectionHandler<DistributionCreatedEventSchema>
{
    private readonly PostgresProjectionStore<DistributionReadModel> _store;

    public DistributionCreatedProjectionHandler(PostgresProjectionStore<DistributionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            DistributionCreatedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"DistributionCreatedProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(DistributionCreatedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(DistributionCreatedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new DistributionReadModel { DistributionId = e.AggregateId };
        state = DistributionProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "DistributionCreatedEvent", eventId, eventVersion, correlationId, ct);
    }
}
