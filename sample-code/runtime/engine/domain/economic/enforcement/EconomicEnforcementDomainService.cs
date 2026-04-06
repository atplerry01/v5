using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.EconomicSystem.Enforcement.Enforcement;
using Whycespace.Domain.SharedKernel.Primitive.Identity;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Economic.Enforcement;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Economic.Enforcement;

public sealed class EconomicEnforcementDomainService : GovernedDomainServiceBase, IEconomicEnforcementDomainService
{
    private readonly IAggregateStore _aggregateStore;

    public EconomicEnforcementDomainService(
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

    public async Task<DomainOperationResult> ApplyAsync(DomainExecutionContext context, string id, string identityId, string reason, string enforcementType, string scope, string duration)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<EconomicEnforcementAggregate>(id);

            var parsedType = Enum.Parse<EnforcementType>(enforcementType);
            var parsedScope = Enum.Parse<EnforcementScope>(scope);
            var parsedDuration = Enum.Parse<EnforcementDuration>(duration);

            aggregate.Apply(
                Guid.Parse(id),
                new IdentityId(Guid.Parse(identityId)),
                parsedType,
                new Reason(reason),
                parsedScope,
                parsedDuration);

            await _aggregateStore.SaveAsync(aggregate);

            var decision = parsedType switch
            {
                EnforcementType.Block => "Deny",
                EnforcementType.Freeze => "Deny",
                EnforcementType.Limit => "Conditional",
                _ => "Deny"
            };

            var reasonCode = $"ENFORCEMENT_{enforcementType.ToUpperInvariant()}_{scope.ToUpperInvariant()}";

            return ((Guid?)Guid.Parse(id), (object?)new EnforcementApplyData(
                id, identityId, enforcementType, scope, duration, "Active", decision, reasonCode));
        });
    }

    public async Task<DomainOperationResult> ReleaseAsync(DomainExecutionContext context, string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<EconomicEnforcementAggregate>(id);
            aggregate.Release();
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)Guid.Parse(id), (object?)new { EnforcementId = id, Status = "Released" });
        });
    }
}
