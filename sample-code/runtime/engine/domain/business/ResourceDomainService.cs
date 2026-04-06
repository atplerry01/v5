using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.BusinessSystem.Resource.Reservation;
using Whycespace.Domain.BusinessSystem.Resource.Capacity;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Resource;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Business;

public sealed class ResourceDomainService : GovernedDomainServiceBase, IResourceDomainService
{
    private readonly IAggregateStore _aggregateStore;

    public ResourceDomainService(
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

    public async Task<DomainOperationResult> CreateReservationAsync(DomainExecutionContext context, string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<ResourceReservationAggregate>(id);
            aggregate.Create(Guid.Parse(id));
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)null, (object?)null);
        });
    }

    public async Task<DomainOperationResult> CreateCapacityAsync(DomainExecutionContext context, string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<ResourceCapacityAggregate>(id);
            aggregate.Create(Guid.Parse(id));
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)null, (object?)null);
        });
    }
}
