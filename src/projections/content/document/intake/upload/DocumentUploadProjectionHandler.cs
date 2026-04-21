using Whycespace.Projections.Content.Document.Intake.Upload.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Document.Intake.Upload;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Document.Intake.Upload;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Document.Intake.Upload;

public sealed class DocumentUploadProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<DocumentUploadRequestedEventSchema>,
    IProjectionHandler<DocumentUploadAcceptedEventSchema>,
    IProjectionHandler<DocumentUploadProcessingStartedEventSchema>,
    IProjectionHandler<DocumentUploadCompletedEventSchema>,
    IProjectionHandler<DocumentUploadFailedEventSchema>,
    IProjectionHandler<DocumentUploadCancelledEventSchema>
{
    private readonly PostgresProjectionStore<DocumentUploadReadModel> _store;

    public DocumentUploadProjectionHandler(PostgresProjectionStore<DocumentUploadReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            DocumentUploadRequestedEventSchema e => Project(e.AggregateId, s => DocumentUploadProjectionReducer.Apply(s, e), "DocumentUploadRequestedEvent", envelope, cancellationToken),
            DocumentUploadAcceptedEventSchema e => Project(e.AggregateId, s => DocumentUploadProjectionReducer.Apply(s, e), "DocumentUploadAcceptedEvent", envelope, cancellationToken),
            DocumentUploadProcessingStartedEventSchema e => Project(e.AggregateId, s => DocumentUploadProjectionReducer.Apply(s, e), "DocumentUploadProcessingStartedEvent", envelope, cancellationToken),
            DocumentUploadCompletedEventSchema e => Project(e.AggregateId, s => DocumentUploadProjectionReducer.Apply(s, e), "DocumentUploadCompletedEvent", envelope, cancellationToken),
            DocumentUploadFailedEventSchema e => Project(e.AggregateId, s => DocumentUploadProjectionReducer.Apply(s, e), "DocumentUploadFailedEvent", envelope, cancellationToken),
            DocumentUploadCancelledEventSchema e => Project(e.AggregateId, s => DocumentUploadProjectionReducer.Apply(s, e), "DocumentUploadCancelledEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"DocumentUploadProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(DocumentUploadRequestedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentUploadProjectionReducer.Apply(s, e), "DocumentUploadRequestedEvent", null, ct);

    public Task HandleAsync(DocumentUploadAcceptedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentUploadProjectionReducer.Apply(s, e), "DocumentUploadAcceptedEvent", null, ct);

    public Task HandleAsync(DocumentUploadProcessingStartedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentUploadProjectionReducer.Apply(s, e), "DocumentUploadProcessingStartedEvent", null, ct);

    public Task HandleAsync(DocumentUploadCompletedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentUploadProjectionReducer.Apply(s, e), "DocumentUploadCompletedEvent", null, ct);

    public Task HandleAsync(DocumentUploadFailedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentUploadProjectionReducer.Apply(s, e), "DocumentUploadFailedEvent", null, ct);

    public Task HandleAsync(DocumentUploadCancelledEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentUploadProjectionReducer.Apply(s, e), "DocumentUploadCancelledEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<DocumentUploadReadModel, DocumentUploadReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new DocumentUploadReadModel { UploadId = aggregateId };
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
