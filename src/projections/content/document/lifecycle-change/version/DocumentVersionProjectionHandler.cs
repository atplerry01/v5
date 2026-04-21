using Whycespace.Projections.Content.Document.LifecycleChange.Version.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Version;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Document.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Document.LifecycleChange.Version;

public sealed class DocumentVersionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<DocumentVersionCreatedEventSchema>,
    IProjectionHandler<DocumentVersionActivatedEventSchema>,
    IProjectionHandler<DocumentVersionSupersededEventSchema>,
    IProjectionHandler<DocumentVersionWithdrawnEventSchema>
{
    private readonly PostgresProjectionStore<DocumentVersionReadModel> _store;

    public DocumentVersionProjectionHandler(PostgresProjectionStore<DocumentVersionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            DocumentVersionCreatedEventSchema e => Project(e.AggregateId, s => DocumentVersionProjectionReducer.Apply(s, e), "DocumentVersionCreatedEvent", envelope, cancellationToken),
            DocumentVersionActivatedEventSchema e => Project(e.AggregateId, s => DocumentVersionProjectionReducer.Apply(s, e), "DocumentVersionActivatedEvent", envelope, cancellationToken),
            DocumentVersionSupersededEventSchema e => Project(e.AggregateId, s => DocumentVersionProjectionReducer.Apply(s, e), "DocumentVersionSupersededEvent", envelope, cancellationToken),
            DocumentVersionWithdrawnEventSchema e => Project(e.AggregateId, s => DocumentVersionProjectionReducer.Apply(s, e), "DocumentVersionWithdrawnEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"DocumentVersionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(DocumentVersionCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentVersionProjectionReducer.Apply(s, e), "DocumentVersionCreatedEvent", null, ct);

    public Task HandleAsync(DocumentVersionActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentVersionProjectionReducer.Apply(s, e), "DocumentVersionActivatedEvent", null, ct);

    public Task HandleAsync(DocumentVersionSupersededEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentVersionProjectionReducer.Apply(s, e), "DocumentVersionSupersededEvent", null, ct);

    public Task HandleAsync(DocumentVersionWithdrawnEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentVersionProjectionReducer.Apply(s, e), "DocumentVersionWithdrawnEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<DocumentVersionReadModel, DocumentVersionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new DocumentVersionReadModel { VersionId = aggregateId };
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
