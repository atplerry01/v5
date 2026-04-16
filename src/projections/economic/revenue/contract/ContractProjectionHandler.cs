using Whycespace.Projections.Economic.Revenue.Contract.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Revenue.Contract;

public sealed class ContractProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<RevenueContractCreatedEventSchema>,
    IProjectionHandler<RevenueContractActivatedEventSchema>,
    IProjectionHandler<RevenueContractTerminatedEventSchema>
{
    private readonly PostgresProjectionStore<ContractReadModel> _store;

    public ContractProjectionHandler(PostgresProjectionStore<ContractReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            RevenueContractCreatedEventSchema e    => ProjectCreated(e, envelope, cancellationToken),
            RevenueContractActivatedEventSchema e  => ProjectActivated(e, envelope, cancellationToken),
            RevenueContractTerminatedEventSchema e => ProjectTerminated(e, envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ContractProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(RevenueContractCreatedEventSchema e, CancellationToken ct = default)
        => await ProjectCreated(e, null, ct);

    public async Task HandleAsync(RevenueContractActivatedEventSchema e, CancellationToken ct = default)
        => await ProjectActivated(e, null, ct);

    public async Task HandleAsync(RevenueContractTerminatedEventSchema e, CancellationToken ct = default)
        => await ProjectTerminated(e, null, ct);

    private async Task ProjectCreated(RevenueContractCreatedEventSchema e, IEventEnvelope? env, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new ContractReadModel { ContractId = e.AggregateId };
        state = ContractProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "RevenueContractCreatedEvent",
            env?.EventId ?? Guid.Empty, env?.SequenceNumber ?? 0, env?.CorrelationId ?? Guid.Empty, ct);
    }

    private async Task ProjectActivated(RevenueContractActivatedEventSchema e, IEventEnvelope? env, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new ContractReadModel { ContractId = e.AggregateId };
        state = ContractProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "RevenueContractActivatedEvent",
            env?.EventId ?? Guid.Empty, env?.SequenceNumber ?? 0, env?.CorrelationId ?? Guid.Empty, ct);
    }

    private async Task ProjectTerminated(RevenueContractTerminatedEventSchema e, IEventEnvelope? env, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new ContractReadModel { ContractId = e.AggregateId };
        state = ContractProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "RevenueContractTerminatedEvent",
            env?.EventId ?? Guid.Empty, env?.SequenceNumber ?? 0, env?.CorrelationId ?? Guid.Empty, ct);
    }
}
