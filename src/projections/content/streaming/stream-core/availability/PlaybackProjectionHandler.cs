using Whycespace.Projections.Content.Streaming.StreamCore.Availability.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Availability;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Availability;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Streaming.StreamCore.Availability;

public sealed class PlaybackProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<PlaybackCreatedEventSchema>,
    IProjectionHandler<PlaybackEnabledEventSchema>,
    IProjectionHandler<PlaybackDisabledEventSchema>,
    IProjectionHandler<PlaybackWindowUpdatedEventSchema>,
    IProjectionHandler<PlaybackArchivedEventSchema>
{
    private readonly PostgresProjectionStore<PlaybackReadModel> _store;

    public PlaybackProjectionHandler(PostgresProjectionStore<PlaybackReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            PlaybackCreatedEventSchema e => Project(e.AggregateId, s => PlaybackProjectionReducer.Apply(s, e), "PlaybackCreatedEvent", envelope, cancellationToken),
            PlaybackEnabledEventSchema e => Project(e.AggregateId, s => PlaybackProjectionReducer.Apply(s, e), "PlaybackEnabledEvent", envelope, cancellationToken),
            PlaybackDisabledEventSchema e => Project(e.AggregateId, s => PlaybackProjectionReducer.Apply(s, e), "PlaybackDisabledEvent", envelope, cancellationToken),
            PlaybackWindowUpdatedEventSchema e => Project(e.AggregateId, s => PlaybackProjectionReducer.Apply(s, e), "PlaybackWindowUpdatedEvent", envelope, cancellationToken),
            PlaybackArchivedEventSchema e => Project(e.AggregateId, s => PlaybackProjectionReducer.Apply(s, e), "PlaybackArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"PlaybackProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(PlaybackCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => PlaybackProjectionReducer.Apply(s, e), "PlaybackCreatedEvent", null, ct);
    public Task HandleAsync(PlaybackEnabledEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => PlaybackProjectionReducer.Apply(s, e), "PlaybackEnabledEvent", null, ct);
    public Task HandleAsync(PlaybackDisabledEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => PlaybackProjectionReducer.Apply(s, e), "PlaybackDisabledEvent", null, ct);
    public Task HandleAsync(PlaybackWindowUpdatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => PlaybackProjectionReducer.Apply(s, e), "PlaybackWindowUpdatedEvent", null, ct);
    public Task HandleAsync(PlaybackArchivedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => PlaybackProjectionReducer.Apply(s, e), "PlaybackArchivedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<PlaybackReadModel, PlaybackReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new PlaybackReadModel { PlaybackId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
