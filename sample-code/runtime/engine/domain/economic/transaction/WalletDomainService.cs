using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.EconomicSystem.Transaction.Wallet;
using Whycespace.Domain.SharedKernel.Primitive.Identity;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Economic.Transaction;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Economic.Transaction;

public sealed class WalletDomainService : GovernedDomainServiceBase, IWalletDomainService
{
    private readonly IAggregateStore _aggregateStore;

    public WalletDomainService(
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

    public async Task<DomainOperationResult> CreateAsync(DomainExecutionContext context, string walletId, string identityId, string currency)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<WalletAggregate>(walletId);

            aggregate.Create(
                Guid.Parse(walletId),
                new IdentityId(Guid.Parse(identityId)),
                new Currency(currency));

            await _aggregateStore.SaveAsync(aggregate);

            return ((Guid?)Guid.Parse(walletId), (object?)new
            {
                WalletId = walletId,
                IdentityId = identityId,
                CurrencyCode = currency,
                Status = "Active"
            });
        });
    }

    public async Task<DomainOperationResult> FreezeAsync(DomainExecutionContext context, string walletId, string reason)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<WalletAggregate>(walletId);
            aggregate.Freeze(reason);
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)Guid.Parse(walletId), (object?)new { WalletId = walletId, Status = "Frozen" });
        });
    }

    public async Task<DomainOperationResult> UnfreezeAsync(DomainExecutionContext context, string walletId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<WalletAggregate>(walletId);
            aggregate.Unfreeze();
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)Guid.Parse(walletId), (object?)new { WalletId = walletId, Status = "Active" });
        });
    }
}
