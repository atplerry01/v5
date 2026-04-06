using Whycespace.Shared.Primitives.Time;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Intelligence.Governance;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Intelligence.Governance;

/// <summary>
/// Runtime implementation of IGovernanceAssistDomainService.
/// Bridges engine layer to domain governance recommendation generation.
/// </summary>
public sealed class GovernanceAssistDomainService : GovernedDomainServiceBase, IGovernanceAssistDomainService
{
    public GovernanceAssistDomainService(
        IPolicyEvaluator policyEvaluator,
        IPolicyDecisionAnchor chainAnchor,
        EnforcementMetrics metrics,
        EnforcementAnomalyEmitter anomalyEmitter,
        IClock clock)
        : base(policyEvaluator, chainAnchor, metrics, anomalyEmitter, clock)
    {
    }

    public async Task<DomainOperationResult> GenerateRecommendationAsync(
        DomainExecutionContext context, Guid id, string area, string proposalType, IReadOnlyDictionary<string, object> inputs)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            return (id, (object?)inputs);
        });
    }

    public async Task<DomainOperationResult> OptimizeAsync(
        DomainExecutionContext context, Guid id, string area, IReadOnlyList<object> recommendations)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            return (id, (object?)null);
        });
    }
}
