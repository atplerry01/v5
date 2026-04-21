using Whycespace.Projections.Content.Document.CoreObject.Record.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Record;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Record;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Document.CoreObject.Record;

public sealed class DocumentRecordProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<DocumentRecordCreatedEventSchema>,
    IProjectionHandler<DocumentRecordLockedEventSchema>,
    IProjectionHandler<DocumentRecordUnlockedEventSchema>,
    IProjectionHandler<DocumentRecordClosedEventSchema>,
    IProjectionHandler<DocumentRecordArchivedEventSchema>
{
    private readonly PostgresProjectionStore<DocumentRecordReadModel> _store;

    public DocumentRecordProjectionHandler(PostgresProjectionStore<DocumentRecordReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            DocumentRecordCreatedEventSchema e => Project(e.AggregateId, s => DocumentRecordProjectionReducer.Apply(s, e), "DocumentRecordCreatedEvent", envelope, cancellationToken),
            DocumentRecordLockedEventSchema e => Project(e.AggregateId, s => DocumentRecordProjectionReducer.Apply(s, e), "DocumentRecordLockedEvent", envelope, cancellationToken),
            DocumentRecordUnlockedEventSchema e => Project(e.AggregateId, s => DocumentRecordProjectionReducer.Apply(s, e), "DocumentRecordUnlockedEvent", envelope, cancellationToken),
            DocumentRecordClosedEventSchema e => Project(e.AggregateId, s => DocumentRecordProjectionReducer.Apply(s, e), "DocumentRecordClosedEvent", envelope, cancellationToken),
            DocumentRecordArchivedEventSchema e => Project(e.AggregateId, s => DocumentRecordProjectionReducer.Apply(s, e), "DocumentRecordArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"DocumentRecordProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(DocumentRecordCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentRecordProjectionReducer.Apply(s, e), "DocumentRecordCreatedEvent", null, ct);

    public Task HandleAsync(DocumentRecordLockedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentRecordProjectionReducer.Apply(s, e), "DocumentRecordLockedEvent", null, ct);

    public Task HandleAsync(DocumentRecordUnlockedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentRecordProjectionReducer.Apply(s, e), "DocumentRecordUnlockedEvent", null, ct);

    public Task HandleAsync(DocumentRecordClosedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentRecordProjectionReducer.Apply(s, e), "DocumentRecordClosedEvent", null, ct);

    public Task HandleAsync(DocumentRecordArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentRecordProjectionReducer.Apply(s, e), "DocumentRecordArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<DocumentRecordReadModel, DocumentRecordReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new DocumentRecordReadModel { RecordId = aggregateId };
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
