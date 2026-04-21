using Whycespace.Projections.Content.Document.CoreObject.Bundle.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Bundle;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Bundle;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Document.CoreObject.Bundle;

public sealed class DocumentBundleProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<DocumentBundleCreatedEventSchema>,
    IProjectionHandler<DocumentBundleRenamedEventSchema>,
    IProjectionHandler<DocumentBundleMemberAddedEventSchema>,
    IProjectionHandler<DocumentBundleMemberRemovedEventSchema>,
    IProjectionHandler<DocumentBundleFinalizedEventSchema>,
    IProjectionHandler<DocumentBundleArchivedEventSchema>
{
    private readonly PostgresProjectionStore<DocumentBundleReadModel> _store;

    public DocumentBundleProjectionHandler(PostgresProjectionStore<DocumentBundleReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            DocumentBundleCreatedEventSchema e => Project(e.AggregateId, s => DocumentBundleProjectionReducer.Apply(s, e), "DocumentBundleCreatedEvent", envelope, cancellationToken),
            DocumentBundleRenamedEventSchema e => Project(e.AggregateId, s => DocumentBundleProjectionReducer.Apply(s, e), "DocumentBundleRenamedEvent", envelope, cancellationToken),
            DocumentBundleMemberAddedEventSchema e => Project(e.AggregateId, s => DocumentBundleProjectionReducer.Apply(s, e), "DocumentBundleMemberAddedEvent", envelope, cancellationToken),
            DocumentBundleMemberRemovedEventSchema e => Project(e.AggregateId, s => DocumentBundleProjectionReducer.Apply(s, e), "DocumentBundleMemberRemovedEvent", envelope, cancellationToken),
            DocumentBundleFinalizedEventSchema e => Project(e.AggregateId, s => DocumentBundleProjectionReducer.Apply(s, e), "DocumentBundleFinalizedEvent", envelope, cancellationToken),
            DocumentBundleArchivedEventSchema e => Project(e.AggregateId, s => DocumentBundleProjectionReducer.Apply(s, e), "DocumentBundleArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"DocumentBundleProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(DocumentBundleCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentBundleProjectionReducer.Apply(s, e), "DocumentBundleCreatedEvent", null, ct);

    public Task HandleAsync(DocumentBundleRenamedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentBundleProjectionReducer.Apply(s, e), "DocumentBundleRenamedEvent", null, ct);

    public Task HandleAsync(DocumentBundleMemberAddedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentBundleProjectionReducer.Apply(s, e), "DocumentBundleMemberAddedEvent", null, ct);

    public Task HandleAsync(DocumentBundleMemberRemovedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentBundleProjectionReducer.Apply(s, e), "DocumentBundleMemberRemovedEvent", null, ct);

    public Task HandleAsync(DocumentBundleFinalizedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentBundleProjectionReducer.Apply(s, e), "DocumentBundleFinalizedEvent", null, ct);

    public Task HandleAsync(DocumentBundleArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentBundleProjectionReducer.Apply(s, e), "DocumentBundleArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<DocumentBundleReadModel, DocumentBundleReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new DocumentBundleReadModel { BundleId = aggregateId };
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
