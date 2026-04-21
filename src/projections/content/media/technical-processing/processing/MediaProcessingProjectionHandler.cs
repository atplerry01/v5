using Whycespace.Projections.Content.Media.TechnicalProcessing.Processing.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Media.TechnicalProcessing.Processing;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Media.TechnicalProcessing.Processing;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Media.TechnicalProcessing.Processing;

public sealed class MediaProcessingProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<MediaProcessingRequestedEventSchema>,
    IProjectionHandler<MediaProcessingStartedEventSchema>,
    IProjectionHandler<MediaProcessingCompletedEventSchema>,
    IProjectionHandler<MediaProcessingFailedEventSchema>,
    IProjectionHandler<MediaProcessingCancelledEventSchema>
{
    private readonly PostgresProjectionStore<MediaProcessingReadModel> _store;
    public MediaProcessingProjectionHandler(PostgresProjectionStore<MediaProcessingReadModel> store) => _store = store;
    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            MediaProcessingRequestedEventSchema e => Project(e.AggregateId, s => MediaProcessingProjectionReducer.Apply(s, e), "MediaProcessingRequestedEvent", envelope, cancellationToken),
            MediaProcessingStartedEventSchema e => Project(e.AggregateId, s => MediaProcessingProjectionReducer.Apply(s, e), "MediaProcessingStartedEvent", envelope, cancellationToken),
            MediaProcessingCompletedEventSchema e => Project(e.AggregateId, s => MediaProcessingProjectionReducer.Apply(s, e), "MediaProcessingCompletedEvent", envelope, cancellationToken),
            MediaProcessingFailedEventSchema e => Project(e.AggregateId, s => MediaProcessingProjectionReducer.Apply(s, e), "MediaProcessingFailedEvent", envelope, cancellationToken),
            MediaProcessingCancelledEventSchema e => Project(e.AggregateId, s => MediaProcessingProjectionReducer.Apply(s, e), "MediaProcessingCancelledEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"MediaProcessingProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(MediaProcessingRequestedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaProcessingProjectionReducer.Apply(s, e), "MediaProcessingRequestedEvent", null, ct);
    public Task HandleAsync(MediaProcessingStartedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaProcessingProjectionReducer.Apply(s, e), "MediaProcessingStartedEvent", null, ct);
    public Task HandleAsync(MediaProcessingCompletedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaProcessingProjectionReducer.Apply(s, e), "MediaProcessingCompletedEvent", null, ct);
    public Task HandleAsync(MediaProcessingFailedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaProcessingProjectionReducer.Apply(s, e), "MediaProcessingFailedEvent", null, ct);
    public Task HandleAsync(MediaProcessingCancelledEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaProcessingProjectionReducer.Apply(s, e), "MediaProcessingCancelledEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<MediaProcessingReadModel, MediaProcessingReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new MediaProcessingReadModel { JobId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
