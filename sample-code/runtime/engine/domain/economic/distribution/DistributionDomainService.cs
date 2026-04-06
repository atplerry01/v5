using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using DomainMoney = Whycespace.Domain.SharedKernel.Primitive.Money.Money;
using DomainCurrency = Whycespace.Domain.SharedKernel.Primitive.Money.Currency;
using Whycespace.Shared.Primitives.Money;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Economic.Capital;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Economic.Distribution;

public sealed class DistributionDomainService : GovernedDomainServiceBase
{
    private readonly IAggregateStore _aggregateStore;
    private readonly DistributionInvariantService _invariantService = new();
    private readonly DistributionVaultRoutingService _vaultRouting = new();
    private readonly IVaultTransferExecutor _vaultTransferExecutor;

    public DistributionDomainService(
        IAggregateStore aggregateStore,
        IVaultTransferExecutor vaultTransferExecutor,
        IPolicyEvaluator policyEvaluator,
        IPolicyDecisionAnchor chainAnchor,
        EnforcementMetrics metrics,
        EnforcementAnomalyEmitter anomalyEmitter,
        IClock clock)
        : base(policyEvaluator, chainAnchor, metrics, anomalyEmitter, clock)
    {
        _aggregateStore = aggregateStore;
        _vaultTransferExecutor = vaultTransferExecutor;
    }

    public async Task<DomainOperationResult> ExecuteAsync(
        DomainExecutionContext context,
        string id,
        Guid revenueId,
        Guid transactionId,
        Money amount,
        IReadOnlyList<DistributionAllocation> allocations)
    {
        var policyInput = new DistributionPolicyInput
        {
            IdentityId = context.ActorId,
            RevenueId = revenueId,
            TransactionId = transactionId,
            Amount = amount,
            Jurisdiction = context.Headers.GetValueOrDefault("X-Jurisdiction")
                ?? throw new InvalidOperationException("Jurisdiction header is required for distribution policy evaluation")
        };

        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<DistributionAggregate>(id);

            var domainAllocations = allocations.Select(a =>
                DistributionAllocation.Create(
                    a.RecipientId,
                    new DomainMoney(
                        a.Amount.Amount,
                        new DomainCurrency(a.Amount.Currency.Code, a.Amount.Currency.Name, a.Amount.Currency.DecimalPlaces))))
                .ToList();

            aggregate.Distribute(
                transactionId,
                domainAllocations,
                _invariantService);

            // E17.H4: Execute vault transfers BEFORE finalizing
            var transfers = _vaultRouting.BuildTransfers(domainAllocations);

            foreach (var transfer in transfers)
            {
                var request = new VaultTransferRequest
                {
                    TransferId = transfer.Id,
                    RecipientId = transfer.RecipientId,
                    Amount = transfer.Amount,
                    CurrencyCode = transfer.CurrencyCode
                };

                var transferResult = await _vaultTransferExecutor.ExecuteTransferAsync(request, context);

                if (!transferResult.Success)
                    throw new InvalidOperationException(
                        $"Vault transfer failed for recipient {transfer.RecipientId}: {transferResult.ErrorMessage}");
            }

            await _aggregateStore.SaveAsync(aggregate);

            return ((Guid?)Guid.Parse(id), (object?)new
            {
                DistributionId = id,
                RevenueId = revenueId,
                TransactionId = transactionId,
                Amount = amount.Amount,
                CurrencyCode = amount.Currency.Code,
                AllocationCount = allocations.Count
            });
        }, policyInput);
    }

    public async Task<DomainOperationResult> ClawbackAsync(
        DomainExecutionContext context,
        string id,
        Guid transactionId)
    {
        var policyInput = new DistributionPolicyInput
        {
            IdentityId = context.ActorId,
            RevenueId = Guid.Empty,
            TransactionId = transactionId,
            Amount = new Money(0, new Whycespace.Shared.Primitives.Money.Currency("USD", "US Dollar", 2)),
            Jurisdiction = context.Headers.GetValueOrDefault("X-Jurisdiction")
                ?? throw new InvalidOperationException("Jurisdiction header is required for distribution policy evaluation")
        };

        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<DistributionAggregate>(id);

            aggregate.Clawback(transactionId);

            await _aggregateStore.SaveAsync(aggregate);

            return ((Guid?)Guid.Parse(id), (object?)new
            {
                DistributionId = id,
                ClawbackTransactionId = transactionId
            });
        }, policyInput);
    }
}
