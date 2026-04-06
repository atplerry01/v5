using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.EconomicSystem.Transaction.Limit;
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

public sealed class LimitDomainService : GovernedDomainServiceBase, ILimitDomainService
{
    private readonly IAggregateStore _aggregateStore;

    public LimitDomainService(
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

    public async Task<DomainOperationResult> CreateAndEvaluateAsync(DomainExecutionContext context, string limitId, string identityId, decimal maxTransactionAmount, decimal dailyLimit, decimal monthlyLimit, decimal transactionAmount, decimal dailyTotal, decimal monthlyTotal)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<EconomicLimitAggregate>(limitId);

            aggregate.Create(
                Guid.Parse(limitId),
                new IdentityId(Guid.Parse(identityId)),
                new MaxTransactionAmount(new Amount(maxTransactionAmount)),
                new DailyLimit(new Amount(dailyLimit)),
                new MonthlyLimit(new Amount(monthlyLimit)));

            aggregate.CheckTransaction(new Amount(transactionAmount));
            aggregate.CheckDailyUsage(new Amount(dailyTotal));
            aggregate.CheckMonthlyUsage(new Amount(monthlyTotal));

            var violations = aggregate.DomainEvents.OfType<LimitExceededEvent>().ToList();

            await _aggregateStore.SaveAsync(aggregate);

            return ((Guid?)Guid.Parse(limitId), (object?)new LimitEvaluationData
            {
                LimitId = limitId,
                IdentityId = identityId,
                HasViolations = violations.Count > 0,
                Violations = violations.Select(v => new LimitViolationData
                {
                    LimitType = v.LimitType
                }).ToList()
            });
        });
    }
}
