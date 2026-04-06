using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.EconomicSystem.Transaction.Transaction;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Economic.Transaction;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Economic.Transaction;

public sealed class TransactionDomainService : GovernedDomainServiceBase, ITransactionDomainService
{
    private readonly IAggregateStore _aggregateStore;

    public TransactionDomainService(
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

    public async Task<DomainOperationResult> InitiateAsync(DomainExecutionContext context, string transactionId, string sourceWalletId, string destinationWalletId, decimal amount, string currencyCode)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<TransactionAggregate>(transactionId);

            aggregate.Initiate(
                Guid.Parse(transactionId),
                new SourceWalletId(Guid.Parse(sourceWalletId)),
                new DestinationWalletId(Guid.Parse(destinationWalletId)),
                new Amount(amount),
                new Currency(currencyCode));

            await _aggregateStore.SaveAsync(aggregate);

            return ((Guid?)Guid.Parse(transactionId), (object?)new
            {
                TransactionId = transactionId,
                SourceWalletId = sourceWalletId,
                DestinationWalletId = destinationWalletId,
                Amount = amount,
                CurrencyCode = currencyCode,
                Status = "Initiated"
            });
        });
    }

    public async Task<DomainOperationResult> ApproveAsync(DomainExecutionContext context, string transactionId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<TransactionAggregate>(transactionId);
            aggregate.Approve();
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)Guid.Parse(transactionId), (object?)null);
        });
    }

    public async Task<DomainOperationResult> CompleteAsync(DomainExecutionContext context, string transactionId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<TransactionAggregate>(transactionId);
            aggregate.Complete();
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)Guid.Parse(transactionId), (object?)new { TransactionId = transactionId, Status = "Completed" });
        });
    }

    public async Task<DomainOperationResult> RejectAsync(DomainExecutionContext context, string transactionId, string reason)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<TransactionAggregate>(transactionId);
            aggregate.Reject(reason);
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)Guid.Parse(transactionId), (object?)new { TransactionId = transactionId, Status = "Rejected", Reason = reason });
        });
    }

    public async Task<DomainOperationResult> SettleAsync(DomainExecutionContext context, string transactionId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<TransactionAggregate>(transactionId);
            aggregate.Settle();
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)Guid.Parse(transactionId), (object?)new { TransactionId = transactionId, Status = "Settled" });
        });
    }
}
