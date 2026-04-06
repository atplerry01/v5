using Whycespace.Domain.ConstitutionalSystem.Policy.Rule;
using Whycespace.Domain.ConstitutionalSystem.Policy.Scope;
using Whycespace.Domain.ConstitutionalSystem.Policy.Version;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Constitutional.Policy;
using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Engine.Domain.Constitutional;

/// <summary>
/// Runtime implementation of IPolicyRegistryDomainService.
/// Bridges engine layer to domain aggregate operations for policy registry.
/// </summary>
public sealed class PolicyRegistryDomainService : GovernedDomainServiceBase, IPolicyRegistryDomainService
{
    private readonly IClock _clock;

    public PolicyRegistryDomainService(
        IPolicyEvaluator policyEvaluator,
        IPolicyDecisionAnchor chainAnchor,
        EnforcementMetrics metrics,
        EnforcementAnomalyEmitter anomalyEmitter,
        IClock clock)
        : base(policyEvaluator, chainAnchor, metrics, anomalyEmitter, clock)
    {
        _clock = clock;
    }

    public async Task<DomainOperationResult> CreatePolicyAsync(DomainExecutionContext context, Guid id, string name, string domain, Guid authorId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var policy = PolicyAggregate.Create(id, name, domain, authorId, _clock.UtcNowOffset);
            return (policy.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> CreatePolicyVersionAsync(DomainExecutionContext context, Guid id, Guid policyId, string rules)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var version = PolicyVersionAggregate.Create(
                id, policyId, 1,
                EffectiveDateRange.Indefinite(_clock.UtcNowOffset),
                rules,
                _clock.UtcNowOffset);
            return (version.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> CreatePolicyScopeAsync(DomainExecutionContext context, Guid id, string cluster, string domain, string contextPath)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var scope = PolicyScopeAggregate.Create(
                id, Guid.Empty, ScopeType.From(cluster), ScopeTarget.For(contextPath), _clock.UtcNowOffset);
            return (scope.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> ActivatePolicyVersionAsync(DomainExecutionContext context, Guid versionId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            return (versionId, (object?)null);
        });
    }

    public async Task<DomainOperationResult> LockPolicyAsync(DomainExecutionContext context, Guid policyId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            return (policyId, (object?)null);
        });
    }

    public Task<bool> CheckScopeAppliesAsync(DomainExecutionContext context, Guid scopeId, string cluster, string domain)
    {
        context.Validate();
        return Task.FromResult(true);
    }

    public Task<bool> CheckVersionEffectiveAsync(DomainExecutionContext context, Guid versionId, DateTimeOffset asOf)
    {
        context.Validate();
        return Task.FromResult(true);
    }
}
