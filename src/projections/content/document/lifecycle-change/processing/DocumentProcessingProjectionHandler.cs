using Whycespace.Projections.Content.Document.LifecycleChange.Processing.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Processing;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Document.LifecycleChange.Processing;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Document.LifecycleChange.Processing;

public sealed class DocumentProcessingProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<DocumentProcessingRequestedEventSchema>,
    IProjectionHandler<DocumentProcessingStartedEventSchema>,
    IProjectionHandler<DocumentProcessingCompletedEventSchema>,
    IProjectionHandler<DocumentProcessingFailedEventSchema>,
    IProjectionHandler<DocumentProcessingCancelledEventSchema>
{
    private readonly PostgresProjectionStore<DocumentProcessingReadModel> _store;

    public DocumentProcessingProjectionHandler(PostgresProjectionStore<DocumentProcessingReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            DocumentProcessingRequestedEventSchema e => Project(e.AggregateId, s => DocumentProcessingProjectionReducer.Apply(s, e), "DocumentProcessingRequestedEvent", envelope, cancellationToken),
            DocumentProcessingStartedEventSchema e => Project(e.AggregateId, s => DocumentProcessingProjectionReducer.Apply(s, e), "DocumentProcessingStartedEvent", envelope, cancellationToken),
            DocumentProcessingCompletedEventSchema e => Project(e.AggregateId, s => DocumentProcessingProjectionReducer.Apply(s, e), "DocumentProcessingCompletedEvent", envelope, cancellationToken),
            DocumentProcessingFailedEventSchema e => Project(e.AggregateId, s => DocumentProcessingProjectionReducer.Apply(s, e), "DocumentProcessingFailedEvent", envelope, cancellationToken),
            DocumentProcessingCancelledEventSchema e => Project(e.AggregateId, s => DocumentProcessingProjectionReducer.Apply(s, e), "DocumentProcessingCancelledEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"DocumentProcessingProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(DocumentProcessingRequestedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentProcessingProjectionReducer.Apply(s, e), "DocumentProcessingRequestedEvent", null, ct);

    public Task HandleAsync(DocumentProcessingStartedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentProcessingProjectionReducer.Apply(s, e), "DocumentProcessingStartedEvent", null, ct);

    public Task HandleAsync(DocumentProcessingCompletedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentProcessingProjectionReducer.Apply(s, e), "DocumentProcessingCompletedEvent", null, ct);

    public Task HandleAsync(DocumentProcessingFailedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentProcessingProjectionReducer.Apply(s, e), "DocumentProcessingFailedEvent", null, ct);

    public Task HandleAsync(DocumentProcessingCancelledEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentProcessingProjectionReducer.Apply(s, e), "DocumentProcessingCancelledEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<DocumentProcessingReadModel, DocumentProcessingReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new DocumentProcessingReadModel { JobId = aggregateId };
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
