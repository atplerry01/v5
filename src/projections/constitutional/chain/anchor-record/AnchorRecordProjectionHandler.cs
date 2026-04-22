using Whycespace.Projections.Constitutional.Chain.AnchorRecord.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Constitutional.Chain.AnchorRecord;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Constitutional.Chain;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Constitutional.Chain.AnchorRecord;

public sealed class AnchorRecordProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AnchorRecordCreatedEventSchema>,
    IProjectionHandler<AnchorRecordSealedEventSchema>
{
    private readonly PostgresProjectionStore<AnchorRecordReadModel> _store;

    public AnchorRecordProjectionHandler(PostgresProjectionStore<AnchorRecordReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AnchorRecordCreatedEventSchema e => Project(e.AggregateId, s => AnchorRecordProjectionReducer.Apply(s, e), "AnchorRecordCreatedEvent", envelope, cancellationToken),
            AnchorRecordSealedEventSchema e => Project(e.AggregateId, s => AnchorRecordProjectionReducer.Apply(s, e), "AnchorRecordSealedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AnchorRecordProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AnchorRecordCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AnchorRecordProjectionReducer.Apply(s, e), "AnchorRecordCreatedEvent", null, ct);

    public Task HandleAsync(AnchorRecordSealedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AnchorRecordProjectionReducer.Apply(s, e), "AnchorRecordSealedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<AnchorRecordReadModel, AnchorRecordReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new AnchorRecordReadModel { AnchorRecordId = aggregateId };
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
