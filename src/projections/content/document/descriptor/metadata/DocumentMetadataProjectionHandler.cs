using Whycespace.Projections.Content.Document.Descriptor.Metadata.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Document.Descriptor.Metadata;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Document.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Document.Descriptor.Metadata;

public sealed class DocumentMetadataProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<DocumentMetadataCreatedEventSchema>,
    IProjectionHandler<DocumentMetadataEntryAddedEventSchema>,
    IProjectionHandler<DocumentMetadataEntryUpdatedEventSchema>,
    IProjectionHandler<DocumentMetadataEntryRemovedEventSchema>,
    IProjectionHandler<DocumentMetadataFinalizedEventSchema>
{
    private readonly PostgresProjectionStore<DocumentMetadataReadModel> _store;

    public DocumentMetadataProjectionHandler(PostgresProjectionStore<DocumentMetadataReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            DocumentMetadataCreatedEventSchema e => Project(e.AggregateId, s => DocumentMetadataProjectionReducer.Apply(s, e), "DocumentMetadataCreatedEvent", envelope, cancellationToken),
            DocumentMetadataEntryAddedEventSchema e => Project(e.AggregateId, s => DocumentMetadataProjectionReducer.Apply(s, e), "DocumentMetadataEntryAddedEvent", envelope, cancellationToken),
            DocumentMetadataEntryUpdatedEventSchema e => Project(e.AggregateId, s => DocumentMetadataProjectionReducer.Apply(s, e), "DocumentMetadataEntryUpdatedEvent", envelope, cancellationToken),
            DocumentMetadataEntryRemovedEventSchema e => Project(e.AggregateId, s => DocumentMetadataProjectionReducer.Apply(s, e), "DocumentMetadataEntryRemovedEvent", envelope, cancellationToken),
            DocumentMetadataFinalizedEventSchema e => Project(e.AggregateId, s => DocumentMetadataProjectionReducer.Apply(s, e), "DocumentMetadataFinalizedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"DocumentMetadataProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(DocumentMetadataCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentMetadataProjectionReducer.Apply(s, e), "DocumentMetadataCreatedEvent", null, ct);

    public Task HandleAsync(DocumentMetadataEntryAddedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentMetadataProjectionReducer.Apply(s, e), "DocumentMetadataEntryAddedEvent", null, ct);

    public Task HandleAsync(DocumentMetadataEntryUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentMetadataProjectionReducer.Apply(s, e), "DocumentMetadataEntryUpdatedEvent", null, ct);

    public Task HandleAsync(DocumentMetadataEntryRemovedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentMetadataProjectionReducer.Apply(s, e), "DocumentMetadataEntryRemovedEvent", null, ct);

    public Task HandleAsync(DocumentMetadataFinalizedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentMetadataProjectionReducer.Apply(s, e), "DocumentMetadataFinalizedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<DocumentMetadataReadModel, DocumentMetadataReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new DocumentMetadataReadModel { MetadataId = aggregateId };
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
