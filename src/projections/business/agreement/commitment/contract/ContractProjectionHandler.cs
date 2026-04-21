using Whycespace.Projections.Business.Agreement.Commitment.Contract.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Contract;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Agreement.Commitment.Contract;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.Commitment.Contract;

public sealed class ContractProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ContractCreatedEventSchema>,
    IProjectionHandler<ContractPartyAddedEventSchema>,
    IProjectionHandler<ContractActivatedEventSchema>,
    IProjectionHandler<ContractSuspendedEventSchema>,
    IProjectionHandler<ContractTerminatedEventSchema>
{
    private readonly PostgresProjectionStore<ContractReadModel> _store;

    public ContractProjectionHandler(PostgresProjectionStore<ContractReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ContractCreatedEventSchema e     => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e), "ContractCreatedEvent",     envelope, cancellationToken),
            ContractPartyAddedEventSchema e  => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e), "ContractPartyAddedEvent",  envelope, cancellationToken),
            ContractActivatedEventSchema e   => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e), "ContractActivatedEvent",   envelope, cancellationToken),
            ContractSuspendedEventSchema e   => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e), "ContractSuspendedEvent",   envelope, cancellationToken),
            ContractTerminatedEventSchema e  => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e), "ContractTerminatedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ContractProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ContractCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e), "ContractCreatedEvent", null, ct);

    public Task HandleAsync(ContractPartyAddedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e), "ContractPartyAddedEvent", null, ct);

    public Task HandleAsync(ContractActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e), "ContractActivatedEvent", null, ct);

    public Task HandleAsync(ContractSuspendedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e), "ContractSuspendedEvent", null, ct);

    public Task HandleAsync(ContractTerminatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ContractProjectionReducer.Apply(s, e), "ContractTerminatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ContractReadModel, ContractReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ContractReadModel { ContractId = aggregateId };
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
