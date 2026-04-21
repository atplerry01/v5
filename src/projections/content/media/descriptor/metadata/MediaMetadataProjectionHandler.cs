using Whycespace.Projections.Content.Media.Descriptor.Metadata.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Media.Descriptor.Metadata;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Media.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Media.Descriptor.Metadata;

public sealed class MediaMetadataProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<MediaMetadataCreatedEventSchema>,
    IProjectionHandler<MediaMetadataEntryAddedEventSchema>,
    IProjectionHandler<MediaMetadataEntryUpdatedEventSchema>,
    IProjectionHandler<MediaMetadataEntryRemovedEventSchema>,
    IProjectionHandler<MediaMetadataFinalizedEventSchema>
{
    private readonly PostgresProjectionStore<MediaMetadataReadModel> _store;
    public MediaMetadataProjectionHandler(PostgresProjectionStore<MediaMetadataReadModel> store) => _store = store;
    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            MediaMetadataCreatedEventSchema e => Project(e.AggregateId, s => MediaMetadataProjectionReducer.Apply(s, e), "MediaMetadataCreatedEvent", envelope, cancellationToken),
            MediaMetadataEntryAddedEventSchema e => Project(e.AggregateId, s => MediaMetadataProjectionReducer.Apply(s, e), "MediaMetadataEntryAddedEvent", envelope, cancellationToken),
            MediaMetadataEntryUpdatedEventSchema e => Project(e.AggregateId, s => MediaMetadataProjectionReducer.Apply(s, e), "MediaMetadataEntryUpdatedEvent", envelope, cancellationToken),
            MediaMetadataEntryRemovedEventSchema e => Project(e.AggregateId, s => MediaMetadataProjectionReducer.Apply(s, e), "MediaMetadataEntryRemovedEvent", envelope, cancellationToken),
            MediaMetadataFinalizedEventSchema e => Project(e.AggregateId, s => MediaMetadataProjectionReducer.Apply(s, e), "MediaMetadataFinalizedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"MediaMetadataProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(MediaMetadataCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaMetadataProjectionReducer.Apply(s, e), "MediaMetadataCreatedEvent", null, ct);
    public Task HandleAsync(MediaMetadataEntryAddedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaMetadataProjectionReducer.Apply(s, e), "MediaMetadataEntryAddedEvent", null, ct);
    public Task HandleAsync(MediaMetadataEntryUpdatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaMetadataProjectionReducer.Apply(s, e), "MediaMetadataEntryUpdatedEvent", null, ct);
    public Task HandleAsync(MediaMetadataEntryRemovedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaMetadataProjectionReducer.Apply(s, e), "MediaMetadataEntryRemovedEvent", null, ct);
    public Task HandleAsync(MediaMetadataFinalizedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaMetadataProjectionReducer.Apply(s, e), "MediaMetadataFinalizedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<MediaMetadataReadModel, MediaMetadataReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new MediaMetadataReadModel { MetadataId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
