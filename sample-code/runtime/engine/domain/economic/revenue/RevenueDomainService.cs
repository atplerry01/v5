using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.EconomicSystem.Revenue.Revenue;
using Whycespace.Domain.EconomicSystem.Ledger.Obligation;
using DomainMoney = Whycespace.Domain.SharedKernel.Primitive.Money.Money;
using DomainCurrency = Whycespace.Domain.SharedKernel.Primitive.Money.Currency;
using Whycespace.Shared.Primitives.Money;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Economic.Revenue;

public sealed class RevenueDomainService : GovernedDomainServiceBase
{
    private readonly IAggregateStore _aggregateStore;
    private readonly RevenueInvariantService _invariantService = new();

    public RevenueDomainService(
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

    public async Task<DomainOperationResult> RecognizeAsync(
        DomainExecutionContext context,
        string id,
        Guid settlementId,
        Money amount,
        ObligationStatus obligationStatus)
    {
        var policyInput = new RevenuePolicyInput
        {
            IdentityId = context.ActorId,
            SettlementId = settlementId,
            TransactionId = Guid.Parse(context.CorrelationId),
            Amount = amount,
            Jurisdiction = context.Headers.GetValueOrDefault("X-Jurisdiction")
                ?? throw new InvalidOperationException("Jurisdiction header is required for revenue policy evaluation")
        };

        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<RevenueAggregate>(id);

            var domainAmount = new DomainMoney(
                amount.Amount,
                new DomainCurrency(amount.Currency.Code, amount.Currency.Name, amount.Currency.DecimalPlaces));

            aggregate.RecognizeRevenue(
                domainAmount,
                obligationStatus,
                _invariantService);

            await _aggregateStore.SaveAsync(aggregate);

            return ((Guid?)Guid.Parse(id), (object?)new
            {
                RevenueId = id,
                SettlementId = settlementId,
                Amount = amount.Amount,
                CurrencyCode = amount.Currency.Code
            });
        }, policyInput);
    }

    public async Task<DomainOperationResult> ReverseAsync(
        DomainExecutionContext context,
        string id,
        Money amount)
    {
        var policyInput = new RevenuePolicyInput
        {
            IdentityId = context.ActorId,
            SettlementId = Guid.Empty,
            TransactionId = Guid.Parse(context.CorrelationId),
            Amount = amount,
            Jurisdiction = context.Headers.GetValueOrDefault("X-Jurisdiction")
                ?? throw new InvalidOperationException("Jurisdiction header is required for revenue policy evaluation")
        };

        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<RevenueAggregate>(id);

            var domainAmount = new DomainMoney(
                amount.Amount,
                new DomainCurrency(amount.Currency.Code, amount.Currency.Name, amount.Currency.DecimalPlaces));

            aggregate.ReverseRevenue(domainAmount);

            await _aggregateStore.SaveAsync(aggregate);

            return ((Guid?)Guid.Parse(id), (object?)new
            {
                RevenueId = id,
                ReversedAmount = amount.Amount,
                CurrencyCode = amount.Currency.Code
            });
        }, policyInput);
    }
}
