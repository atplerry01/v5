using Whycespace.Projections.Content.Streaming.LiveStreaming.Archive.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Archive;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Streaming.LiveStreaming.Archive;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Streaming.LiveStreaming.Archive;

public sealed class ArchiveProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ArchiveStartedEventSchema>,
    IProjectionHandler<ArchiveCompletedEventSchema>,
    IProjectionHandler<ArchiveFailedEventSchema>,
    IProjectionHandler<ArchiveFinalizedEventSchema>,
    IProjectionHandler<ArchiveArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ArchiveReadModel> _store;

    public ArchiveProjectionHandler(PostgresProjectionStore<ArchiveReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            ArchiveStartedEventSchema e => Project(e.AggregateId, s => ArchiveProjectionReducer.Apply(s, e), "ArchiveStartedEvent", envelope, cancellationToken),
            ArchiveCompletedEventSchema e => Project(e.AggregateId, s => ArchiveProjectionReducer.Apply(s, e), "ArchiveCompletedEvent", envelope, cancellationToken),
            ArchiveFailedEventSchema e => Project(e.AggregateId, s => ArchiveProjectionReducer.Apply(s, e), "ArchiveFailedEvent", envelope, cancellationToken),
            ArchiveFinalizedEventSchema e => Project(e.AggregateId, s => ArchiveProjectionReducer.Apply(s, e), "ArchiveFinalizedEvent", envelope, cancellationToken),
            ArchiveArchivedEventSchema e => Project(e.AggregateId, s => ArchiveProjectionReducer.Apply(s, e), "ArchiveArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ArchiveProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(ArchiveStartedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ArchiveProjectionReducer.Apply(s, e), "ArchiveStartedEvent", null, ct);
    public Task HandleAsync(ArchiveCompletedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ArchiveProjectionReducer.Apply(s, e), "ArchiveCompletedEvent", null, ct);
    public Task HandleAsync(ArchiveFailedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ArchiveProjectionReducer.Apply(s, e), "ArchiveFailedEvent", null, ct);
    public Task HandleAsync(ArchiveFinalizedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ArchiveProjectionReducer.Apply(s, e), "ArchiveFinalizedEvent", null, ct);
    public Task HandleAsync(ArchiveArchivedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ArchiveProjectionReducer.Apply(s, e), "ArchiveArchivedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<ArchiveReadModel, ArchiveReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new ArchiveReadModel { ArchiveId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
