using Whycespace.Shared.Primitives.Time;
using Whycespace.Shared.Primitives.Money;
using Whycespace.Domain.StructuralSystem.Cluster.Spv.Transaction;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Structural;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Structural.CrossSpv;

public sealed class CrossSpvDomainService : GovernedDomainServiceBase, ICrossSpvDomainService
{
    private readonly IAggregateStore _aggregateStore;

    public CrossSpvDomainService(
        IAggregateStore aggregateStore,
        IPolicyEvaluator policyEvaluator,
        IPolicyDecisionAnchor chainAnchor,
        EnforcementMetrics metrics,
        EnforcementAnomalyEmitter anomalyEmitter,
        IClock clock)
        : base(policyEvaluator, chainAnchor, metrics, anomalyEmitter, clock)
    {
        _aggregateStore = aggregateStore;
    }

    public async Task<DomainOperationResult> CreateCrossSpvTransactionAsync(
        DomainExecutionContext context,
        string id,
        string rootSpvId,
        IReadOnlyList<SpvLegDto> legs)
    {
        var domainLegs = legs.Select(l =>
            SpvLeg.Create(
                l.FromSpvId,
                l.ToSpvId,
                new Whycespace.Domain.SharedKernel.Primitive.Money.Money(
                    l.Amount,
                    new Whycespace.Domain.SharedKernel.Primitive.Money.Currency(l.CurrencyCode))))
            .ToList();

        var totalAmount = legs.Aggregate(0m, (acc, l) => acc + l.Amount);

        // H6: Full per-leg visibility for policy evaluation
        var policyInput = new CrossSpvPolicyInput
        {
            IdentityId = context.ActorId,
            TransactionId = Guid.Parse(id),
            RootSpvId = Guid.Parse(rootSpvId),
            Legs = legs.Select(l => new SpvLegPolicy
            {
                FromSpvId = l.FromSpvId,
                ToSpvId = l.ToSpvId,
                Amount = new Money(l.Amount, new Currency(l.CurrencyCode))
            }).ToList(),
            TotalAmount = new Money(totalAmount, new Currency(legs[0].CurrencyCode)),
            Jurisdiction = context.Domain
        };

        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = CrossSpvTransactionAggregate.Create(
                Guid.Parse(id),
                Guid.Parse(rootSpvId),
                domainLegs);
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        }, policyInput);
    }

    public async Task<DomainOperationResult> PrepareCrossSpvTransactionAsync(
        DomainExecutionContext context,
        string id,
        string transactionId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<CrossSpvTransactionAggregate>(id);
            aggregate.Prepare(Guid.Parse(transactionId));
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> CommitCrossSpvTransactionAsync(
        DomainExecutionContext context,
        string id,
        string transactionId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<CrossSpvTransactionAggregate>(id);
            aggregate.Commit(Guid.Parse(transactionId));
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> FailCrossSpvTransactionAsync(
        DomainExecutionContext context,
        string id,
        string transactionId,
        string reason)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<CrossSpvTransactionAggregate>(id);
            aggregate.Fail(Guid.Parse(transactionId), reason);
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> SetExecutionStateAsync(
        DomainExecutionContext context,
        string id,
        string state)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<CrossSpvTransactionAggregate>(id);
            var executionState = state switch
            {
                "preparing" => CrossSpvExecutionState.Preparing,
                "executing" => CrossSpvExecutionState.Executing,
                "committing" => CrossSpvExecutionState.Committing,
                "compensating" => CrossSpvExecutionState.Compensating,
                "completed" => CrossSpvExecutionState.Completed,
                "failed" => CrossSpvExecutionState.Failed,
                _ => CrossSpvExecutionState.Pending
            };
            aggregate.SetExecutionState(executionState);
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }
}
