using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.StructuralSystem.Cluster.Cluster;
using Whycespace.Domain.StructuralSystem.Cluster.Authority;
using Whycespace.Domain.StructuralSystem.Cluster.Topology;
using Whycespace.Domain.StructuralSystem.Cluster.Spv;
using SubClusterNs = Whycespace.Domain.StructuralSystem.Cluster.SubCluster;
using Whycespace.Domain.StructuralSystem.Cluster.Classification;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Structural;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Structural;

public sealed class ClusterDomainService : GovernedDomainServiceBase, IClusterDomainService
{
    private readonly IAggregateStore _aggregateStore;

    public ClusterDomainService(
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

    // ── E18.1 — Cluster Core ────────────────────────────────────

    public async Task<DomainOperationResult> CreateClusterAsync(
        DomainExecutionContext context, string id, string name, string jurisdiction)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = WhyceClusterAggregate.Create(Guid.Parse(id), name, jurisdiction);
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        }, new ClusterPolicyInput
        {
            IdentityId = context.ActorId,
            EntityId = Guid.Parse(id),
            Jurisdiction = jurisdiction
        });
    }

    public async Task<DomainOperationResult> ActivateClusterAsync(
        DomainExecutionContext context, string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<WhyceClusterAggregate>(id);
            aggregate.Activate();
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> AddClusterAuthorityAsync(
        DomainExecutionContext context, string clusterId, string authorityId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<WhyceClusterAggregate>(clusterId);
            aggregate.AddAuthority(Guid.Parse(authorityId));
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    // ── E18.2 — Authority ───────────────────────────────────────

    public async Task<DomainOperationResult> CreateAuthorityAsync(
        DomainExecutionContext context, string id, string clusterId, string name)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = ClusterAuthorityAggregate.Create(
                Guid.Parse(id), Guid.Parse(clusterId), name);
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        }, new AuthorityPolicyInput
        {
            IdentityId = context.ActorId,
            EntityId = Guid.Parse(id),
            Jurisdiction = context.Domain
        });
    }

    public async Task<DomainOperationResult> AddAuthoritySubClusterAsync(
        DomainExecutionContext context, string authorityId, string subClusterId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<ClusterAuthorityAggregate>(authorityId);
            aggregate.AddSubCluster(Guid.Parse(subClusterId));
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    // ── E18.3 — Topology (SubCluster) ───────────────────────────

    public async Task<DomainOperationResult> CreateTopologyAsync(
        DomainExecutionContext context, string id, string authorityId, string name)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = SubClusterAggregate.Create(
                Guid.Parse(id), Guid.Parse(authorityId), name);
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        }, new SubClusterPolicyInput
        {
            IdentityId = context.ActorId,
            EntityId = Guid.Parse(id),
            Jurisdiction = context.Domain
        });
    }

    public async Task<DomainOperationResult> AddTopologySpvAsync(
        DomainExecutionContext context, string subClusterId, string spvId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<SubClusterAggregate>(subClusterId);
            aggregate.AddSpv(Guid.Parse(spvId));
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    // ── E18.4 — SPV ─────────────────────────────────────────────

    public async Task<DomainOperationResult> CreateSpvAsync(
        DomainExecutionContext context, string id, string subClusterId, string name)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = SpvAggregate.Create(
                Guid.Parse(id), Guid.Parse(subClusterId), name);
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        }, new SpvPolicyInput
        {
            IdentityId = context.ActorId,
            EntityId = Guid.Parse(id),
            Jurisdiction = context.Domain
        });
    }

    public async Task<DomainOperationResult> ActivateSpvAsync(
        DomainExecutionContext context, string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<SpvAggregate>(id);
            aggregate.Activate();
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> AddSpvOperatorAsync(
        DomainExecutionContext context, string spvId, string operatorId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<SpvAggregate>(spvId);
            aggregate.AddOperator(Guid.Parse(operatorId));
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> ReplaceSpvOperatorAsync(
        DomainExecutionContext context, string spvId, string oldOperatorId, string newOperatorId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<SpvAggregate>(spvId);
            aggregate.ReplaceOperator(Guid.Parse(oldOperatorId), Guid.Parse(newOperatorId));
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    // ── E18.4b — SPV Lifecycle ────────────────────────────────────

    public async Task<DomainOperationResult> SuspendSpvAsync(
        DomainExecutionContext context, string id, string reason)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<SpvAggregate>(id);
            aggregate.Suspend(reason);
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> ReactivateSpvAsync(
        DomainExecutionContext context, string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<SpvAggregate>(id);
            aggregate.Reactivate();
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> TerminateSpvAsync(
        DomainExecutionContext context, string id, string reason)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<SpvAggregate>(id);
            aggregate.Terminate(reason, DateTimeOffset.UtcNow);
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> CloseSpvAsync(
        DomainExecutionContext context, string id, string auditRecordId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<SpvAggregate>(id);
            aggregate.Close(Guid.Parse(auditRecordId), DateTimeOffset.UtcNow);
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    // ── E18.5 — SubCluster ─────────────────────────────────────

    public async Task<DomainOperationResult> CreateSubClusterAsync(
        DomainExecutionContext context, string id, string authorityId, string name)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = SubClusterNs.SubClusterAggregate.Create(
                Guid.Parse(id), Guid.Parse(authorityId), name);
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        }, new SubClusterPolicyInput
        {
            IdentityId = context.ActorId,
            EntityId = Guid.Parse(id),
            Jurisdiction = context.Domain
        });
    }

    public async Task<DomainOperationResult> ActivateSubClusterAsync(
        DomainExecutionContext context, string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<SubClusterNs.SubClusterAggregate>(id);
            aggregate.Activate();
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> DeactivateSubClusterAsync(
        DomainExecutionContext context, string id, string reason)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<SubClusterNs.SubClusterAggregate>(id);
            aggregate.Deactivate(reason);
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> AddSubClusterSpvAsync(
        DomainExecutionContext context, string subClusterId, string spvId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<SubClusterNs.SubClusterAggregate>(subClusterId);
            aggregate.AddSpv(Guid.Parse(spvId));
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    public async Task<DomainOperationResult> RemoveSubClusterSpvAsync(
        DomainExecutionContext context, string subClusterId, string spvId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<SubClusterNs.SubClusterAggregate>(subClusterId);
            aggregate.RemoveSpv(Guid.Parse(spvId));
            await _aggregateStore.SaveAsync(aggregate);
            return (aggregate.Id, (object?)null);
        });
    }

    // ── Legacy (classification) ─────────────────────────────────

    public async Task<DomainOperationResult> CreateClassificationAsync(
        DomainExecutionContext context, string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<ClusterClassificationAggregate>(id);
            aggregate.Create(Guid.Parse(id));
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)null, (object?)null);
        });
    }
}
