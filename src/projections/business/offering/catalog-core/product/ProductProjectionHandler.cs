using Whycespace.Projections.Business.Offering.CatalogCore.Product.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Product;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.Product;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Offering.CatalogCore.Product;

public sealed class ProductProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ProductCreatedEventSchema>,
    IProjectionHandler<ProductUpdatedEventSchema>,
    IProjectionHandler<ProductActivatedEventSchema>,
    IProjectionHandler<ProductArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ProductReadModel> _store;

    public ProductProjectionHandler(PostgresProjectionStore<ProductReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ProductCreatedEventSchema e    => Project(e.AggregateId, s => ProductProjectionReducer.Apply(s, e), "ProductCreatedEvent",    envelope, cancellationToken),
            ProductUpdatedEventSchema e    => Project(e.AggregateId, s => ProductProjectionReducer.Apply(s, e), "ProductUpdatedEvent",    envelope, cancellationToken),
            ProductActivatedEventSchema e  => Project(e.AggregateId, s => ProductProjectionReducer.Apply(s, e), "ProductActivatedEvent",  envelope, cancellationToken),
            ProductArchivedEventSchema e   => Project(e.AggregateId, s => ProductProjectionReducer.Apply(s, e), "ProductArchivedEvent",   envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ProductProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ProductCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProductProjectionReducer.Apply(s, e), "ProductCreatedEvent", null, ct);

    public Task HandleAsync(ProductUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProductProjectionReducer.Apply(s, e), "ProductUpdatedEvent", null, ct);

    public Task HandleAsync(ProductActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProductProjectionReducer.Apply(s, e), "ProductActivatedEvent", null, ct);

    public Task HandleAsync(ProductArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProductProjectionReducer.Apply(s, e), "ProductArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ProductReadModel, ProductReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ProductReadModel { ProductId = aggregateId };
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
