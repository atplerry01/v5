using Whycespace.Projections.Constitutional.Chain.EvidenceRecord.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Constitutional.Chain.EvidenceRecord;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Constitutional.Chain;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Constitutional.Chain.EvidenceRecord;

public sealed class EvidenceRecordProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<EvidenceRecordCreatedEventSchema>,
    IProjectionHandler<EvidenceRecordArchivedEventSchema>
{
    private readonly PostgresProjectionStore<EvidenceRecordReadModel> _store;

    public EvidenceRecordProjectionHandler(PostgresProjectionStore<EvidenceRecordReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            EvidenceRecordCreatedEventSchema e => Project(e.AggregateId, s => EvidenceRecordProjectionReducer.Apply(s, e), "EvidenceRecordCreatedEvent", envelope, cancellationToken),
            EvidenceRecordArchivedEventSchema e => Project(e.AggregateId, s => EvidenceRecordProjectionReducer.Apply(s, e), "EvidenceRecordArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"EvidenceRecordProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(EvidenceRecordCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => EvidenceRecordProjectionReducer.Apply(s, e), "EvidenceRecordCreatedEvent", null, ct);

    public Task HandleAsync(EvidenceRecordArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => EvidenceRecordProjectionReducer.Apply(s, e), "EvidenceRecordArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<EvidenceRecordReadModel, EvidenceRecordReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new EvidenceRecordReadModel { EvidenceRecordId = aggregateId };
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
