using Whycespace.Projections.Business.Offering.CatalogCore.Bundle.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Bundle;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.Bundle;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Offering.CatalogCore.Bundle;

public sealed class BundleProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<BundleCreatedEventSchema>,
    IProjectionHandler<BundleMemberAddedEventSchema>,
    IProjectionHandler<BundleMemberRemovedEventSchema>,
    IProjectionHandler<BundleActivatedEventSchema>,
    IProjectionHandler<BundleArchivedEventSchema>
{
    private readonly PostgresProjectionStore<BundleReadModel> _store;

    public BundleProjectionHandler(PostgresProjectionStore<BundleReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            BundleCreatedEventSchema e        => Project(e.AggregateId, s => BundleProjectionReducer.Apply(s, e), "BundleCreatedEvent",        envelope, cancellationToken),
            BundleMemberAddedEventSchema e    => Project(e.AggregateId, s => BundleProjectionReducer.Apply(s, e), "BundleMemberAddedEvent",    envelope, cancellationToken),
            BundleMemberRemovedEventSchema e  => Project(e.AggregateId, s => BundleProjectionReducer.Apply(s, e), "BundleMemberRemovedEvent",  envelope, cancellationToken),
            BundleActivatedEventSchema e      => Project(e.AggregateId, s => BundleProjectionReducer.Apply(s, e), "BundleActivatedEvent",      envelope, cancellationToken),
            BundleArchivedEventSchema e       => Project(e.AggregateId, s => BundleProjectionReducer.Apply(s, e), "BundleArchivedEvent",       envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"BundleProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(BundleCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => BundleProjectionReducer.Apply(s, e), "BundleCreatedEvent", null, ct);

    public Task HandleAsync(BundleMemberAddedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => BundleProjectionReducer.Apply(s, e), "BundleMemberAddedEvent", null, ct);

    public Task HandleAsync(BundleMemberRemovedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => BundleProjectionReducer.Apply(s, e), "BundleMemberRemovedEvent", null, ct);

    public Task HandleAsync(BundleActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => BundleProjectionReducer.Apply(s, e), "BundleActivatedEvent", null, ct);

    public Task HandleAsync(BundleArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => BundleProjectionReducer.Apply(s, e), "BundleArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<BundleReadModel, BundleReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new BundleReadModel { BundleId = aggregateId };
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
