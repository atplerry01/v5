using Whycespace.Projections.Content.Media.Asset.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Media.Asset;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Media.Asset;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Media.Asset.Item;

public sealed class MediaAssetProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<MediaAssetRegisteredEventSchema>,
    IProjectionHandler<MediaAssetProcessingStartedEventSchema>,
    IProjectionHandler<MediaAssetAvailableEventSchema>,
    IProjectionHandler<MediaAssetPublishedEventSchema>,
    IProjectionHandler<MediaAssetUnpublishedEventSchema>,
    IProjectionHandler<MediaAssetArchivedEventSchema>,
    IProjectionHandler<MediaAssetMetadataUpdatedEventSchema>
{
    private readonly PostgresProjectionStore<MediaAssetReadModel> _store;

    public MediaAssetProjectionHandler(PostgresProjectionStore<MediaAssetReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            MediaAssetRegisteredEventSchema e => Project(e.AggregateId, s => MediaAssetProjectionReducer.Apply(s, e), "MediaAssetRegisteredEvent", envelope, cancellationToken),
            MediaAssetProcessingStartedEventSchema e => Project(e.AggregateId, s => MediaAssetProjectionReducer.Apply(s, e), "MediaAssetProcessingStartedEvent", envelope, cancellationToken),
            MediaAssetAvailableEventSchema e => Project(e.AggregateId, s => MediaAssetProjectionReducer.Apply(s, e), "MediaAssetAvailableEvent", envelope, cancellationToken),
            MediaAssetPublishedEventSchema e => Project(e.AggregateId, s => MediaAssetProjectionReducer.Apply(s, e), "MediaAssetPublishedEvent", envelope, cancellationToken),
            MediaAssetUnpublishedEventSchema e => Project(e.AggregateId, s => MediaAssetProjectionReducer.Apply(s, e), "MediaAssetUnpublishedEvent", envelope, cancellationToken),
            MediaAssetArchivedEventSchema e => Project(e.AggregateId, s => MediaAssetProjectionReducer.Apply(s, e), "MediaAssetArchivedEvent", envelope, cancellationToken),
            MediaAssetMetadataUpdatedEventSchema e => Project(e.AggregateId, s => MediaAssetProjectionReducer.Apply(s, e), "MediaAssetMetadataUpdatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"MediaAssetProjectionHandler received unmatched event type: {envelope.Payload.GetType().Name}. " +
                $"EventId={envelope.EventId}, EventType={envelope.EventType}.")
        };
    }

    public Task HandleAsync(MediaAssetRegisteredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaAssetProjectionReducer.Apply(s, e), "MediaAssetRegisteredEvent", null, ct);
    public Task HandleAsync(MediaAssetProcessingStartedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaAssetProjectionReducer.Apply(s, e), "MediaAssetProcessingStartedEvent", null, ct);
    public Task HandleAsync(MediaAssetAvailableEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaAssetProjectionReducer.Apply(s, e), "MediaAssetAvailableEvent", null, ct);
    public Task HandleAsync(MediaAssetPublishedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaAssetProjectionReducer.Apply(s, e), "MediaAssetPublishedEvent", null, ct);
    public Task HandleAsync(MediaAssetUnpublishedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaAssetProjectionReducer.Apply(s, e), "MediaAssetUnpublishedEvent", null, ct);
    public Task HandleAsync(MediaAssetArchivedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaAssetProjectionReducer.Apply(s, e), "MediaAssetArchivedEvent", null, ct);
    public Task HandleAsync(MediaAssetMetadataUpdatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaAssetProjectionReducer.Apply(s, e), "MediaAssetMetadataUpdatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<MediaAssetReadModel, MediaAssetReadModel> reduce,
        string eventType,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new MediaAssetReadModel { Id = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(
            aggregateId,
            state,
            eventType,
            envelope?.EventId ?? Guid.Empty,
            envelope?.SequenceNumber ?? 0,
            envelope?.CorrelationId ?? Guid.Empty,
            ct);
    }
}
