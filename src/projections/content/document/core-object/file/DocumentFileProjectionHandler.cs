using Whycespace.Projections.Content.Document.CoreObject.File.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.File;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.File;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Document.CoreObject.File;

public sealed class DocumentFileProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<DocumentFileRegisteredEventSchema>,
    IProjectionHandler<DocumentFileIntegrityVerifiedEventSchema>,
    IProjectionHandler<DocumentFileSupersededEventSchema>,
    IProjectionHandler<DocumentFileArchivedEventSchema>
{
    private readonly PostgresProjectionStore<DocumentFileReadModel> _store;

    public DocumentFileProjectionHandler(PostgresProjectionStore<DocumentFileReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            DocumentFileRegisteredEventSchema e => Project(e.AggregateId, s => DocumentFileProjectionReducer.Apply(s, e), "DocumentFileRegisteredEvent", envelope, cancellationToken),
            DocumentFileIntegrityVerifiedEventSchema e => Project(e.AggregateId, s => DocumentFileProjectionReducer.Apply(s, e), "DocumentFileIntegrityVerifiedEvent", envelope, cancellationToken),
            DocumentFileSupersededEventSchema e => Project(e.AggregateId, s => DocumentFileProjectionReducer.Apply(s, e), "DocumentFileSupersededEvent", envelope, cancellationToken),
            DocumentFileArchivedEventSchema e => Project(e.AggregateId, s => DocumentFileProjectionReducer.Apply(s, e), "DocumentFileArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"DocumentFileProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(DocumentFileRegisteredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentFileProjectionReducer.Apply(s, e), "DocumentFileRegisteredEvent", null, ct);

    public Task HandleAsync(DocumentFileIntegrityVerifiedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentFileProjectionReducer.Apply(s, e), "DocumentFileIntegrityVerifiedEvent", null, ct);

    public Task HandleAsync(DocumentFileSupersededEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentFileProjectionReducer.Apply(s, e), "DocumentFileSupersededEvent", null, ct);

    public Task HandleAsync(DocumentFileArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentFileProjectionReducer.Apply(s, e), "DocumentFileArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<DocumentFileReadModel, DocumentFileReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new DocumentFileReadModel { DocumentFileId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(
            aggregateId,
            state,
            eventTypeName,
            envelope?.EventId ?? Guid.Empty,
            envelope?.SequenceNumber ?? 0,
            envelope?.CorrelationId ?? Guid.Empty,
            ct);
    }
}
