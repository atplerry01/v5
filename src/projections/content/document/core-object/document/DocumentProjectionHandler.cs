using Whycespace.Projections.Content.Document.CoreObject.Document.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Document;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Document;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Document.CoreObject.Document;

public sealed class DocumentProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<DocumentCreatedEventSchema>,
    IProjectionHandler<DocumentMetadataUpdatedEventSchema>,
    IProjectionHandler<DocumentVersionAttachedEventSchema>,
    IProjectionHandler<DocumentActivatedEventSchema>,
    IProjectionHandler<DocumentArchivedEventSchema>,
    IProjectionHandler<DocumentRestoredEventSchema>,
    IProjectionHandler<DocumentSupersededEventSchema>
{
    private readonly PostgresProjectionStore<DocumentReadModel> _store;

    public DocumentProjectionHandler(PostgresProjectionStore<DocumentReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            DocumentCreatedEventSchema e => Project(e.AggregateId, s => DocumentProjectionReducer.Apply(s, e), "DocumentCreatedEvent", envelope, cancellationToken),
            DocumentMetadataUpdatedEventSchema e => Project(e.AggregateId, s => DocumentProjectionReducer.Apply(s, e), "DocumentMetadataUpdatedEvent", envelope, cancellationToken),
            DocumentVersionAttachedEventSchema e => Project(e.AggregateId, s => DocumentProjectionReducer.Apply(s, e), "DocumentVersionAttachedEvent", envelope, cancellationToken),
            DocumentActivatedEventSchema e => Project(e.AggregateId, s => DocumentProjectionReducer.Apply(s, e), "DocumentActivatedEvent", envelope, cancellationToken),
            DocumentArchivedEventSchema e => Project(e.AggregateId, s => DocumentProjectionReducer.Apply(s, e), "DocumentArchivedEvent", envelope, cancellationToken),
            DocumentRestoredEventSchema e => Project(e.AggregateId, s => DocumentProjectionReducer.Apply(s, e), "DocumentRestoredEvent", envelope, cancellationToken),
            DocumentSupersededEventSchema e => Project(e.AggregateId, s => DocumentProjectionReducer.Apply(s, e), "DocumentSupersededEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"DocumentProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(DocumentCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentProjectionReducer.Apply(s, e), "DocumentCreatedEvent", null, ct);

    public Task HandleAsync(DocumentMetadataUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentProjectionReducer.Apply(s, e), "DocumentMetadataUpdatedEvent", null, ct);

    public Task HandleAsync(DocumentVersionAttachedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentProjectionReducer.Apply(s, e), "DocumentVersionAttachedEvent", null, ct);

    public Task HandleAsync(DocumentActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentProjectionReducer.Apply(s, e), "DocumentActivatedEvent", null, ct);

    public Task HandleAsync(DocumentArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentProjectionReducer.Apply(s, e), "DocumentArchivedEvent", null, ct);

    public Task HandleAsync(DocumentRestoredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentProjectionReducer.Apply(s, e), "DocumentRestoredEvent", null, ct);

    public Task HandleAsync(DocumentSupersededEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentProjectionReducer.Apply(s, e), "DocumentSupersededEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<DocumentReadModel, DocumentReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new DocumentReadModel { DocumentId = aggregateId };
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
