using Whycespace.Projections.Content.Document.CoreObject.Template.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Template;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Template;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Document.CoreObject.Template;

public sealed class DocumentTemplateProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<DocumentTemplateCreatedEventSchema>,
    IProjectionHandler<DocumentTemplateUpdatedEventSchema>,
    IProjectionHandler<DocumentTemplateActivatedEventSchema>,
    IProjectionHandler<DocumentTemplateDeprecatedEventSchema>,
    IProjectionHandler<DocumentTemplateArchivedEventSchema>
{
    private readonly PostgresProjectionStore<DocumentTemplateReadModel> _store;

    public DocumentTemplateProjectionHandler(PostgresProjectionStore<DocumentTemplateReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            DocumentTemplateCreatedEventSchema e => Project(e.AggregateId, s => DocumentTemplateProjectionReducer.Apply(s, e), "DocumentTemplateCreatedEvent", envelope, cancellationToken),
            DocumentTemplateUpdatedEventSchema e => Project(e.AggregateId, s => DocumentTemplateProjectionReducer.Apply(s, e), "DocumentTemplateUpdatedEvent", envelope, cancellationToken),
            DocumentTemplateActivatedEventSchema e => Project(e.AggregateId, s => DocumentTemplateProjectionReducer.Apply(s, e), "DocumentTemplateActivatedEvent", envelope, cancellationToken),
            DocumentTemplateDeprecatedEventSchema e => Project(e.AggregateId, s => DocumentTemplateProjectionReducer.Apply(s, e), "DocumentTemplateDeprecatedEvent", envelope, cancellationToken),
            DocumentTemplateArchivedEventSchema e => Project(e.AggregateId, s => DocumentTemplateProjectionReducer.Apply(s, e), "DocumentTemplateArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"DocumentTemplateProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(DocumentTemplateCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentTemplateProjectionReducer.Apply(s, e), "DocumentTemplateCreatedEvent", null, ct);

    public Task HandleAsync(DocumentTemplateUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentTemplateProjectionReducer.Apply(s, e), "DocumentTemplateUpdatedEvent", null, ct);

    public Task HandleAsync(DocumentTemplateActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentTemplateProjectionReducer.Apply(s, e), "DocumentTemplateActivatedEvent", null, ct);

    public Task HandleAsync(DocumentTemplateDeprecatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentTemplateProjectionReducer.Apply(s, e), "DocumentTemplateDeprecatedEvent", null, ct);

    public Task HandleAsync(DocumentTemplateArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DocumentTemplateProjectionReducer.Apply(s, e), "DocumentTemplateArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<DocumentTemplateReadModel, DocumentTemplateReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new DocumentTemplateReadModel { TemplateId = aggregateId };
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
