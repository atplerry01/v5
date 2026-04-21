using Whycespace.Projections.Content.Media.CoreObject.Asset.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Asset;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Media.CoreObject.Asset;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Media.CoreObject.Asset;

public sealed class AssetProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AssetCreatedEventSchema>,
    IProjectionHandler<AssetRenamedEventSchema>,
    IProjectionHandler<AssetReclassifiedEventSchema>,
    IProjectionHandler<AssetActivatedEventSchema>,
    IProjectionHandler<AssetRetiredEventSchema>,
    IProjectionHandler<AssetKindAssignedEventSchema>
{
    private readonly PostgresProjectionStore<AssetReadModel> _store;

    public AssetProjectionHandler(PostgresProjectionStore<AssetReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            AssetCreatedEventSchema e => Project(e.AggregateId, s => AssetProjectionReducer.Apply(s, e), "AssetCreatedEvent", envelope, cancellationToken),
            AssetRenamedEventSchema e => Project(e.AggregateId, s => AssetProjectionReducer.Apply(s, e), "AssetRenamedEvent", envelope, cancellationToken),
            AssetReclassifiedEventSchema e => Project(e.AggregateId, s => AssetProjectionReducer.Apply(s, e), "AssetReclassifiedEvent", envelope, cancellationToken),
            AssetActivatedEventSchema e => Project(e.AggregateId, s => AssetProjectionReducer.Apply(s, e), "AssetActivatedEvent", envelope, cancellationToken),
            AssetRetiredEventSchema e => Project(e.AggregateId, s => AssetProjectionReducer.Apply(s, e), "AssetRetiredEvent", envelope, cancellationToken),
            AssetKindAssignedEventSchema e => Project(e.AggregateId, s => AssetProjectionReducer.Apply(s, e), "AssetKindAssignedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AssetProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(AssetCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AssetProjectionReducer.Apply(s, e), "AssetCreatedEvent", null, ct);
    public Task HandleAsync(AssetRenamedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AssetProjectionReducer.Apply(s, e), "AssetRenamedEvent", null, ct);
    public Task HandleAsync(AssetReclassifiedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AssetProjectionReducer.Apply(s, e), "AssetReclassifiedEvent", null, ct);
    public Task HandleAsync(AssetActivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AssetProjectionReducer.Apply(s, e), "AssetActivatedEvent", null, ct);
    public Task HandleAsync(AssetRetiredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AssetProjectionReducer.Apply(s, e), "AssetRetiredEvent", null, ct);
    public Task HandleAsync(AssetKindAssignedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => AssetProjectionReducer.Apply(s, e), "AssetKindAssignedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<AssetReadModel, AssetReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new AssetReadModel { AssetId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
