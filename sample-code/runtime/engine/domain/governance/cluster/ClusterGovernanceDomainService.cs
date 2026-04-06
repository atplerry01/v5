using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.DecisionSystem.Governance.ClusterDecision;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Governance;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Governance.Cluster;

/// <summary>
/// E18.7.5 — Governed domain service for cluster governance decisions.
/// All operations are policy-gated and chain-anchored via GovernedDomainServiceBase.
/// </summary>
public sealed class ClusterGovernanceDomainService : GovernedDomainServiceBase, IClusterGovernanceDomainService
{
    private readonly IAggregateStore _aggregateStore;

    public ClusterGovernanceDomainService(
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

    public async Task<DomainOperationResult> ProposeAsync(
        DomainExecutionContext context,
        string id,
        string clusterId,
        string decisionType,
        string decisionHash)
    {
        var policyInput = new ClusterGovernanceInput
        {
            ClusterId = Guid.Parse(clusterId),
            DecisionType = decisionType,
            EconomicImpact = 0m,
            Jurisdiction = context.Domain,
            IdentityId = context.ActorId
        };

        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = ClusterGovernanceDecisionAggregate.Propose(
                Guid.Parse(id),
                Guid.Parse(clusterId),
                decisionType,
                decisionHash);
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        }, policyInput);
    }

    public async Task<DomainOperationResult> ApproveAsync(
        DomainExecutionContext context,
        string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<ClusterGovernanceDecisionAggregate>(id);
            aggregate.Approve();
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> ExecuteAsync(
        DomainExecutionContext context,
        string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<ClusterGovernanceDecisionAggregate>(id);
            aggregate.Execute();
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> RejectAsync(
        DomainExecutionContext context,
        string id,
        string reason)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<ClusterGovernanceDecisionAggregate>(id);
            aggregate.Reject(reason);
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }
}
