using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Constitutional.Policy;
using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Runtime.Engine.Domain.Constitutional;

/// <summary>
/// Runtime implementation of IPolicyEnforcementDomainService.
/// Bridges engine layer to domain enforcement action creation.
/// </summary>
public sealed class PolicyEnforcementDomainService : GovernedDomainServiceBase, IPolicyEnforcementDomainService
{
    private readonly IClock _clock;

    public PolicyEnforcementDomainService(
        IPolicyEvaluator policyEvaluator,
        IPolicyDecisionAnchor chainAnchor,
        EnforcementMetrics metrics,
        EnforcementAnomalyEmitter anomalyEmitter,
        IClock clock)
        : base(policyEvaluator, chainAnchor, metrics, anomalyEmitter, clock)
    {
        _clock = clock;
    }

    public async Task<DomainOperationResult> CreateEnforcementActionAsync(
        DomainExecutionContext context, Guid id, string actionType, string targetId, string reason)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var action = EnforcementAction.Create(
                id,
                EnforcementType.From(actionType),
                EnforcementSeverity.Soft,
                EnforcementTargetType.Identity,
                targetId,
                reason,
                DeterministicIdHelper.FromSeed($"{context.CorrelationId}:{id}:{actionType}:{targetId}:enforcement").ToString("N"),
                _clock.UtcNowOffset);

            return (action.ActionId, (object?)null);
        });
    }
}
