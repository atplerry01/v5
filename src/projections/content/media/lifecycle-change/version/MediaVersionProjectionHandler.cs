using Whycespace.Projections.Content.Media.LifecycleChange.Version.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Media.LifecycleChange.Version;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Media.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Media.LifecycleChange.Version;

public sealed class MediaVersionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<MediaVersionCreatedEventSchema>,
    IProjectionHandler<MediaVersionActivatedEventSchema>,
    IProjectionHandler<MediaVersionSupersededEventSchema>,
    IProjectionHandler<MediaVersionWithdrawnEventSchema>
{
    private readonly PostgresProjectionStore<MediaVersionReadModel> _store;
    public MediaVersionProjectionHandler(PostgresProjectionStore<MediaVersionReadModel> store) => _store = store;
    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            MediaVersionCreatedEventSchema e => Project(e.AggregateId, s => MediaVersionProjectionReducer.Apply(s, e), "MediaVersionCreatedEvent", envelope, cancellationToken),
            MediaVersionActivatedEventSchema e => Project(e.AggregateId, s => MediaVersionProjectionReducer.Apply(s, e), "MediaVersionActivatedEvent", envelope, cancellationToken),
            MediaVersionSupersededEventSchema e => Project(e.AggregateId, s => MediaVersionProjectionReducer.Apply(s, e), "MediaVersionSupersededEvent", envelope, cancellationToken),
            MediaVersionWithdrawnEventSchema e => Project(e.AggregateId, s => MediaVersionProjectionReducer.Apply(s, e), "MediaVersionWithdrawnEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"MediaVersionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(MediaVersionCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaVersionProjectionReducer.Apply(s, e), "MediaVersionCreatedEvent", null, ct);
    public Task HandleAsync(MediaVersionActivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaVersionProjectionReducer.Apply(s, e), "MediaVersionActivatedEvent", null, ct);
    public Task HandleAsync(MediaVersionSupersededEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaVersionProjectionReducer.Apply(s, e), "MediaVersionSupersededEvent", null, ct);
    public Task HandleAsync(MediaVersionWithdrawnEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => MediaVersionProjectionReducer.Apply(s, e), "MediaVersionWithdrawnEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<MediaVersionReadModel, MediaVersionReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new MediaVersionReadModel { VersionId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
