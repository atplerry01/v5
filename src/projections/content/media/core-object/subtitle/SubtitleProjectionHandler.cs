using Whycespace.Projections.Content.Media.CoreObject.Subtitle.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Subtitle;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Media.CoreObject.Subtitle;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Media.CoreObject.Subtitle;

public sealed class SubtitleProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SubtitleCreatedEventSchema>,
    IProjectionHandler<SubtitleUpdatedEventSchema>,
    IProjectionHandler<SubtitleFinalizedEventSchema>,
    IProjectionHandler<SubtitleArchivedEventSchema>
{
    private readonly PostgresProjectionStore<SubtitleReadModel> _store;
    public SubtitleProjectionHandler(PostgresProjectionStore<SubtitleReadModel> store) => _store = store;
    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            SubtitleCreatedEventSchema e => Project(e.AggregateId, s => SubtitleProjectionReducer.Apply(s, e), "SubtitleCreatedEvent", envelope, cancellationToken),
            SubtitleUpdatedEventSchema e => Project(e.AggregateId, s => SubtitleProjectionReducer.Apply(s, e), "SubtitleUpdatedEvent", envelope, cancellationToken),
            SubtitleFinalizedEventSchema e => Project(e.AggregateId, s => SubtitleProjectionReducer.Apply(s, e), "SubtitleFinalizedEvent", envelope, cancellationToken),
            SubtitleArchivedEventSchema e => Project(e.AggregateId, s => SubtitleProjectionReducer.Apply(s, e), "SubtitleArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"SubtitleProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(SubtitleCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SubtitleProjectionReducer.Apply(s, e), "SubtitleCreatedEvent", null, ct);
    public Task HandleAsync(SubtitleUpdatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SubtitleProjectionReducer.Apply(s, e), "SubtitleUpdatedEvent", null, ct);
    public Task HandleAsync(SubtitleFinalizedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SubtitleProjectionReducer.Apply(s, e), "SubtitleFinalizedEvent", null, ct);
    public Task HandleAsync(SubtitleArchivedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SubtitleProjectionReducer.Apply(s, e), "SubtitleArchivedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<SubtitleReadModel, SubtitleReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new SubtitleReadModel { SubtitleId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
