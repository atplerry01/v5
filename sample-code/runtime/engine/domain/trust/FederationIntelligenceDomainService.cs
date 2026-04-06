using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.TrustSystem.Identity.Federation;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Trust;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Trust;

/// <summary>
/// Runtime implementation of IFederationIntelligenceDomainService.
/// Bridges engine layer to domain trust normalization and reputation calculations.
/// </summary>
public sealed class FederationIntelligenceDomainService : GovernedDomainServiceBase, IFederationIntelligenceDomainService
{
    private static readonly IReadOnlyDictionary<string, decimal> TypeFactors =
        new Dictionary<string, decimal>
        {
            ["Government"] = 1.0m,
            ["Financial"] = 0.85m,
            ["Enterprise"] = 0.70m,
            ["Platform"] = 0.55m,
        };

    public FederationIntelligenceDomainService(
        IPolicyEvaluator policyEvaluator,
        IPolicyDecisionAnchor chainAnchor,
        EnforcementMetrics metrics,
        EnforcementAnomalyEmitter anomalyEmitter,
        IClock clock)
        : base(policyEvaluator, chainAnchor, metrics, anomalyEmitter, clock)
    {
    }

    public decimal NormalizeTrustScore(
        DomainExecutionContext context,
        decimal rawScore, string issuerType, decimal reputationScore,
        string trajectoryTrend, decimal volatility)
    {
        context.Validate();

        if (!TypeFactors.TryGetValue(issuerType, out var typeFactor))
            typeFactor = 0.50m;

        var reputationFactor = 0.5m + 0.5m * (decimal)Math.Log(1.0 + (double)reputationScore) /
            (decimal)Math.Log(101.0);

        var trajectoryFactor = trajectoryTrend switch
        {
            "Degrading" => Math.Max(0.7m - volatility * 0.3m, 0.5m),
            "Stable" => 1.0m,
            "Improving" => Math.Min(1.0m + volatility * 0.1m, 1.1m),
            _ => 1.0m
        };

        return Math.Clamp(rawScore * typeFactor * reputationFactor * trajectoryFactor, 0m, 100m);
    }

    public decimal ComputeIssuerReputation(
        DomainExecutionContext context,
        decimal credentialValidityRate, decimal revocationRate, decimal incidentRate,
        string trajectoryTrend, decimal volatility)
    {
        context.Validate();

        decimal score = 100m;
        score -= (1m - credentialValidityRate) * 30m;
        score -= revocationRate * 25m;
        score -= Math.Min(incidentRate * 20m, 30m);

        if (trajectoryTrend == "Degrading")
            score -= 10m;

        score -= volatility * 15m;

        return Math.Clamp(score, 0m, 100m);
    }
}
