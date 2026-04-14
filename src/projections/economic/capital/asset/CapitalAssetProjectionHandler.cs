using Whycespace.Projections.Economic.Capital.Asset.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Capital.Asset;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Capital.Asset;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Capital.Asset;

public sealed class CapitalAssetProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AssetCreatedEventSchema>,
    IProjectionHandler<AssetValuedEventSchema>,
    IProjectionHandler<AssetDisposedEventSchema>
{
    private readonly PostgresProjectionStore<CapitalAssetReadModel> _store;

    public CapitalAssetProjectionHandler(PostgresProjectionStore<CapitalAssetReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AssetCreatedEventSchema e => Project(e.AggregateId, s => CapitalAssetProjectionReducer.Apply(s, e), "AssetCreatedEvent", envelope, cancellationToken),
            AssetValuedEventSchema e => Project(e.AggregateId, s => CapitalAssetProjectionReducer.Apply(s, e), "AssetValuedEvent", envelope, cancellationToken),
            AssetDisposedEventSchema e => Project(e.AggregateId, s => CapitalAssetProjectionReducer.Apply(s, e), "AssetDisposedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"CapitalAssetProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AssetCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalAssetProjectionReducer.Apply(s, e), "AssetCreatedEvent", null, ct);

    public Task HandleAsync(AssetValuedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalAssetProjectionReducer.Apply(s, e), "AssetValuedEvent", null, ct);

    public Task HandleAsync(AssetDisposedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalAssetProjectionReducer.Apply(s, e), "AssetDisposedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<CapitalAssetReadModel, CapitalAssetReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new CapitalAssetReadModel { AssetId = aggregateId };
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
