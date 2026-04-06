using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;
using Whycespace.Domain.ConstitutionalSystem.Policy.Rule;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Constitutional;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Constitutional;

public sealed class PolicyDomainService : GovernedDomainServiceBase, IPolicyDomainService
{
    private readonly IAggregateStore _aggregateStore;

    public PolicyDomainService(
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

    public async Task<DomainOperationResult> CreateEnforcementAsync(DomainExecutionContext context, string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<PolicyEnforcementAggregate>(id);
            aggregate.Create(Guid.Parse(id));
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)Guid.Parse(id), (object?)null);
        });
    }

    public async Task<DomainOperationResult> CreateRuleAsync(DomainExecutionContext context, string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<PolicyRuleAggregate>(id);
            aggregate.Create(Guid.Parse(id));
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)Guid.Parse(id), (object?)null);
        });
    }
}
