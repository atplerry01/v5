using Whycespace.Projections.Content.Streaming.StreamCore.Manifest.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Manifest;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Manifest;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Streaming.StreamCore.Manifest;

public sealed class ManifestProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ManifestCreatedEventSchema>,
    IProjectionHandler<ManifestUpdatedEventSchema>,
    IProjectionHandler<ManifestPublishedEventSchema>,
    IProjectionHandler<ManifestRetiredEventSchema>,
    IProjectionHandler<ManifestArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ManifestReadModel> _store;

    public ManifestProjectionHandler(PostgresProjectionStore<ManifestReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            ManifestCreatedEventSchema e => Project(e.AggregateId, s => ManifestProjectionReducer.Apply(s, e), "ManifestCreatedEvent", envelope, cancellationToken),
            ManifestUpdatedEventSchema e => Project(e.AggregateId, s => ManifestProjectionReducer.Apply(s, e), "ManifestUpdatedEvent", envelope, cancellationToken),
            ManifestPublishedEventSchema e => Project(e.AggregateId, s => ManifestProjectionReducer.Apply(s, e), "ManifestPublishedEvent", envelope, cancellationToken),
            ManifestRetiredEventSchema e => Project(e.AggregateId, s => ManifestProjectionReducer.Apply(s, e), "ManifestRetiredEvent", envelope, cancellationToken),
            ManifestArchivedEventSchema e => Project(e.AggregateId, s => ManifestProjectionReducer.Apply(s, e), "ManifestArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ManifestProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(ManifestCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ManifestProjectionReducer.Apply(s, e), "ManifestCreatedEvent", null, ct);
    public Task HandleAsync(ManifestUpdatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ManifestProjectionReducer.Apply(s, e), "ManifestUpdatedEvent", null, ct);
    public Task HandleAsync(ManifestPublishedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ManifestProjectionReducer.Apply(s, e), "ManifestPublishedEvent", null, ct);
    public Task HandleAsync(ManifestRetiredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ManifestProjectionReducer.Apply(s, e), "ManifestRetiredEvent", null, ct);
    public Task HandleAsync(ManifestArchivedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ManifestProjectionReducer.Apply(s, e), "ManifestArchivedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<ManifestReadModel, ManifestReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new ManifestReadModel { ManifestId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
