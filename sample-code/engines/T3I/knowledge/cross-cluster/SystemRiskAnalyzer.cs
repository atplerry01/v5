namespace Whycespace.Engines.T3I.Knowledge.CrossCluster;

/// <summary>
/// Produces a system-wide risk score combining multiple risk dimensions.
/// Stateless, deterministic — no persistence, no external calls.
/// </summary>
public sealed class SystemRiskAnalyzer
{
    /// <summary>
    /// Computes an overall system risk assessment from multiple dimensions.
    /// </summary>
    public SystemRiskAssessment Assess(SystemRiskCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var dimensions = new List<RiskDimension>();

        // Policy risk
        var policyRisk = Math.Min(1.0m, command.PolicyViolationRate / 0.3m);
        dimensions.Add(new RiskDimension("Policy", policyRisk, Weight: 0.25m));

        // Economic risk
        var economicRisk = Math.Min(1.0m, command.RevenueVolatility / 0.5m);
        dimensions.Add(new RiskDimension("Economic", economicRisk, Weight: 0.30m));

        // Operational risk
        var operationalRisk = Math.Min(1.0m, command.SystemErrorRate / 0.2m);
        dimensions.Add(new RiskDimension("Operational", operationalRisk, Weight: 0.25m));

        // Governance risk
        var governanceRisk = Math.Min(1.0m, command.PendingProposalCount / 10.0m);
        dimensions.Add(new RiskDimension("Governance", governanceRisk, Weight: 0.20m));

        // Weighted composite
        var compositeScore = dimensions.Sum(d => d.Score * d.Weight);

        var riskLevel = compositeScore switch
        {
            >= 0.8m => "Critical",
            >= 0.6m => "High",
            >= 0.35m => "Medium",
            >= 0.15m => "Low",
            _ => "Minimal"
        };

        return new SystemRiskAssessment(
            CompositeScore: compositeScore,
            RiskLevel: riskLevel,
            Dimensions: dimensions,
            RequiresEscalation: compositeScore >= 0.6m);
    }
}

public sealed record SystemRiskCommand(
    decimal PolicyViolationRate,
    decimal RevenueVolatility,
    decimal SystemErrorRate,
    int PendingProposalCount);

public sealed record SystemRiskAssessment(
    decimal CompositeScore,
    string RiskLevel,
    IReadOnlyList<RiskDimension> Dimensions,
    bool RequiresEscalation);

public sealed record RiskDimension(
    string Name,
    decimal Score,
    decimal Weight);
