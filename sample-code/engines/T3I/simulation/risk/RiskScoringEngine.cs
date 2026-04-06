using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Utils;

namespace Whycespace.Engines.T3I.Simulation;

/// <summary>
/// T3I engine: computes a risk score (0.0–1.0) from multiple factors.
/// Deterministic — same inputs always produce the same score.
/// NEVER writes to chain or mutates state.
///
/// Factors: trustScore, amount, policy sensitivity, anomaly signals.
/// </summary>
public sealed class RiskScoringEngine : IEngine<ComputeRiskScoreCommand>
{
    public Task<EngineResult> ExecuteAsync(
        ComputeRiskScoreCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default)
    {
        var factors = new List<RiskContribution>();
        var totalWeight = 0.0;
        var weightedSum = 0.0;

        // Factor 1: Trust score (inverted — low trust = high risk)
        var trustRisk = 1.0 - TrustScoreNormalizer.Normalize(command.TrustScore);
        factors.Add(new RiskContribution("trust_score", trustRisk, 0.30, "Identity trust level"));
        weightedSum += trustRisk * 0.30;
        totalWeight += 0.30;

        // Factor 2: Transaction amount risk
        var amountRisk = ComputeAmountRisk(command.Amount, command.Currency);
        factors.Add(new RiskContribution("transaction_amount", amountRisk, 0.25, "Transaction size relative to thresholds"));
        weightedSum += amountRisk * 0.25;
        totalWeight += 0.25;

        // Factor 3: Policy sensitivity
        var policySensitivity = command.PolicyAllowed ? 0.0 : 0.8;
        factors.Add(new RiskContribution("policy_sensitivity", policySensitivity, 0.25, "Policy evaluation outcome"));
        weightedSum += policySensitivity * 0.25;
        totalWeight += 0.25;

        // Factor 4: Anomaly signals
        var anomalyRisk = 0.0;
        if (command.HasEconomicThresholdViolation) anomalyRisk += 0.4;
        if (command.HasWorkflowBlock) anomalyRisk += 0.3;
        anomalyRisk = Math.Min(anomalyRisk, 1.0);
        factors.Add(new RiskContribution("anomaly_signals", anomalyRisk, 0.20, "Economic/workflow anomalies"));
        weightedSum += anomalyRisk * 0.20;
        totalWeight += 0.20;

        var riskScore = totalWeight > 0 ? Math.Round(weightedSum / totalWeight, 4, MidpointRounding.ToEven) : 0.0;
        riskScore = Math.Clamp(riskScore, 0.0, 1.0);

        var category = riskScore switch
        {
            < 0.2 => "low",
            < 0.5 => "medium",
            < 0.8 => "high",
            _ => "critical"
        };

        var result = new RiskAssessmentResult(
            RiskScore: riskScore,
            Category: category,
            Factors: factors,
            SubjectId: command.SubjectId,
            Action: command.Action);

        return Task.FromResult(EngineResult.Ok(result));
    }

    private static double ComputeAmountRisk(decimal amount, string currency)
    {
        var normalized = (double)AmountNormalizer.Normalize(amount, currency);
        return normalized switch
        {
            <= 0 => 0.0,
            <= 10_000 => 0.1,
            <= 100_000 => 0.3,
            <= 1_000_000 => 0.6,
            _ => 0.9
        };
    }
}

public sealed record ComputeRiskScoreCommand(
    string SubjectId,
    string Action,
    double TrustScore,
    decimal Amount,
    string Currency,
    bool PolicyAllowed,
    bool HasEconomicThresholdViolation,
    bool HasWorkflowBlock);

public sealed record RiskAssessmentResult(
    double RiskScore,
    string Category,
    List<RiskContribution> Factors,
    string SubjectId,
    string Action);

public sealed record RiskContribution(
    string Factor,
    double Score,
    double Weight,
    string Description);
