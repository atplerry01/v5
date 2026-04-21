using Whycespace.Projections.Content.Media.Intake.Ingest.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Media.Intake.Ingest;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Media.Intake.Ingest;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Media.Intake.Ingest;

public sealed class MediaIngestProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<MediaIngestRequestedEventSchema>,
    IProjectionHandler<MediaIngestAcceptedEventSchema>,
    IProjectionHandler<MediaIngestProcessingStartedEventSchema>,
    IProjectionHandler<MediaIngestCompletedEventSchema>,
    IProjectionHandler<MediaIngestFailedEventSchema>,
    IProjectionHandler<MediaIngestCancelledEventSchema>
{
    private readonly PostgresProjectionStore<MediaIngestReadModel> _store;
    public MediaIngestProjectionHandler(PostgresProjectionStore<MediaIngestReadModel> store) => _store = store;
    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            MediaIngestRequestedEventSchema e => Project(e.AggregateId, s => MediaIngestProjectionReducer.Apply(s, e), "MediaIngestRequestedEvent", envelope, cancellationToken),
            MediaIngestAcceptedEventSchema e => Project(e.AggregateId, s => MediaIngestProjectionReducer.Apply(s, e), "MediaIngestAcceptedEvent", envelope, cancellationToken),
            MediaIngestProcessingStartedEventSchema e => Project(e.AggregateId, s => MediaIngestProjectionReducer.Apply(s, e), "MediaIngestProcessingStartedEvent", envelope, cancellationToken),
            MediaIngestCompletedEventSchema e => Project(e.AggregateId, s => MediaIngestProjectionReducer.Apply(s, e), "MediaIngestCompletedEvent", envelope, cancellationToken),
            MediaIngestFailedEventSchema e => Project(e.AggregateId, s => MediaIngestProjectionReducer.Apply(s, e), "MediaIngestFailedEvent", envelope, cancellationToken),
            MediaIngestCancelledEventSchema e => Project(e.AggregateId, s => MediaIngestProjectionReducer.Apply(s, e), "MediaIngestCancelledEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"MediaIngestProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(MediaIngestRequestedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaIngestProjectionReducer.Apply(s, e), "MediaIngestRequestedEvent", null, ct);
    public Task HandleAsync(MediaIngestAcceptedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaIngestProjectionReducer.Apply(s, e), "MediaIngestAcceptedEvent", null, ct);
    public Task HandleAsync(MediaIngestProcessingStartedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaIngestProjectionReducer.Apply(s, e), "MediaIngestProcessingStartedEvent", null, ct);
    public Task HandleAsync(MediaIngestCompletedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaIngestProjectionReducer.Apply(s, e), "MediaIngestCompletedEvent", null, ct);
    public Task HandleAsync(MediaIngestFailedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaIngestProjectionReducer.Apply(s, e), "MediaIngestFailedEvent", null, ct);
    public Task HandleAsync(MediaIngestCancelledEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaIngestProjectionReducer.Apply(s, e), "MediaIngestCancelledEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<MediaIngestReadModel, MediaIngestReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new MediaIngestReadModel { UploadId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
