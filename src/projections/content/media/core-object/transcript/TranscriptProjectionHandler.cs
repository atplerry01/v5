using Whycespace.Projections.Content.Media.CoreObject.Transcript.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Transcript;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Media.CoreObject.Transcript;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Media.CoreObject.Transcript;

public sealed class TranscriptProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<TranscriptCreatedEventSchema>,
    IProjectionHandler<TranscriptUpdatedEventSchema>,
    IProjectionHandler<TranscriptFinalizedEventSchema>,
    IProjectionHandler<TranscriptArchivedEventSchema>
{
    private readonly PostgresProjectionStore<TranscriptReadModel> _store;
    public TranscriptProjectionHandler(PostgresProjectionStore<TranscriptReadModel> store) => _store = store;
    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            TranscriptCreatedEventSchema e => Project(e.AggregateId, s => TranscriptProjectionReducer.Apply(s, e), "TranscriptCreatedEvent", envelope, cancellationToken),
            TranscriptUpdatedEventSchema e => Project(e.AggregateId, s => TranscriptProjectionReducer.Apply(s, e), "TranscriptUpdatedEvent", envelope, cancellationToken),
            TranscriptFinalizedEventSchema e => Project(e.AggregateId, s => TranscriptProjectionReducer.Apply(s, e), "TranscriptFinalizedEvent", envelope, cancellationToken),
            TranscriptArchivedEventSchema e => Project(e.AggregateId, s => TranscriptProjectionReducer.Apply(s, e), "TranscriptArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"TranscriptProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(TranscriptCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => TranscriptProjectionReducer.Apply(s, e), "TranscriptCreatedEvent", null, ct);
    public Task HandleAsync(TranscriptUpdatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => TranscriptProjectionReducer.Apply(s, e), "TranscriptUpdatedEvent", null, ct);
    public Task HandleAsync(TranscriptFinalizedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => TranscriptProjectionReducer.Apply(s, e), "TranscriptFinalizedEvent", null, ct);
    public Task HandleAsync(TranscriptArchivedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => TranscriptProjectionReducer.Apply(s, e), "TranscriptArchivedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<TranscriptReadModel, TranscriptReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new TranscriptReadModel { TranscriptId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
