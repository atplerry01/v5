using Whycespace.Projections.Business.Offering.CatalogCore.Catalog.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Offering.CatalogCore.Catalog;

public sealed class CatalogProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<CatalogCreatedEventSchema>,
    IProjectionHandler<CatalogMemberAddedEventSchema>,
    IProjectionHandler<CatalogMemberRemovedEventSchema>,
    IProjectionHandler<CatalogPublishedEventSchema>,
    IProjectionHandler<CatalogArchivedEventSchema>
{
    private readonly PostgresProjectionStore<CatalogReadModel> _store;

    public CatalogProjectionHandler(PostgresProjectionStore<CatalogReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            CatalogCreatedEventSchema e        => Project(e.AggregateId, s => CatalogProjectionReducer.Apply(s, e), "CatalogCreatedEvent",        envelope, cancellationToken),
            CatalogMemberAddedEventSchema e    => Project(e.AggregateId, s => CatalogProjectionReducer.Apply(s, e), "CatalogMemberAddedEvent",    envelope, cancellationToken),
            CatalogMemberRemovedEventSchema e  => Project(e.AggregateId, s => CatalogProjectionReducer.Apply(s, e), "CatalogMemberRemovedEvent",  envelope, cancellationToken),
            CatalogPublishedEventSchema e      => Project(e.AggregateId, s => CatalogProjectionReducer.Apply(s, e), "CatalogPublishedEvent",      envelope, cancellationToken),
            CatalogArchivedEventSchema e       => Project(e.AggregateId, s => CatalogProjectionReducer.Apply(s, e), "CatalogArchivedEvent",       envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"CatalogProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(CatalogCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CatalogProjectionReducer.Apply(s, e), "CatalogCreatedEvent", null, ct);

    public Task HandleAsync(CatalogMemberAddedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CatalogProjectionReducer.Apply(s, e), "CatalogMemberAddedEvent", null, ct);

    public Task HandleAsync(CatalogMemberRemovedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CatalogProjectionReducer.Apply(s, e), "CatalogMemberRemovedEvent", null, ct);

    public Task HandleAsync(CatalogPublishedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CatalogProjectionReducer.Apply(s, e), "CatalogPublishedEvent", null, ct);

    public Task HandleAsync(CatalogArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CatalogProjectionReducer.Apply(s, e), "CatalogArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<CatalogReadModel, CatalogReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new CatalogReadModel { CatalogId = aggregateId };
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
