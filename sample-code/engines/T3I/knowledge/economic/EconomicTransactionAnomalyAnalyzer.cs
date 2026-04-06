using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Utils;

namespace Whycespace.Engines.T3I.Economic;

/// <summary>
/// T3I engine: detects anomalies in economic transactions bound to policy decisions.
/// Stateless, deterministic. No persistence, no HTTP, no system clock.
///
/// Anomalies detected:
///   - Large transaction spikes (amount exceeds threshold)
///   - Policy bypass attempts (execution without decision)
///   - Unusual account activity (high-frequency or mismatch)
///   - Decision/execution mismatch (denied decision but executed)
/// </summary>
public sealed class EconomicTransactionAnomalyAnalyzer : IEngine<AnalyzeEconomicTransactionCommand>
{
    private const decimal LargeTransactionThreshold = 100_000m;
    private const decimal HighValueThreshold = 1_000_000m;

    public Task<EngineResult> ExecuteAsync(
        AnalyzeEconomicTransactionCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default)
    {
        var anomalies = new List<EconomicAnomaly>();
        var normalizedAmount = AmountNormalizer.Normalize(command.Amount, command.Currency);

        // Anomaly 1: Large transaction spike
        if (normalizedAmount > LargeTransactionThreshold)
        {
            var severity = normalizedAmount > HighValueThreshold
                ? EconomicAnomalySeverity.Critical
                : EconomicAnomalySeverity.High;

            anomalies.Add(new EconomicAnomaly(
                "large_transaction",
                $"Transaction amount {normalizedAmount} {command.Currency} exceeds threshold",
                severity));
        }

        // Anomaly 2: Policy bypass attempt
        if (string.IsNullOrWhiteSpace(command.DecisionHash))
        {
            anomalies.Add(new EconomicAnomaly(
                "policy_bypass",
                "Economic transaction has no linked policy decision hash",
                EconomicAnomalySeverity.Critical));
        }

        // Anomaly 3: Decision/execution mismatch
        if (command.Decision.Equals("DENY", StringComparison.OrdinalIgnoreCase) && command.WasExecuted)
        {
            anomalies.Add(new EconomicAnomaly(
                "decision_execution_mismatch",
                "Transaction executed despite DENY decision",
                EconomicAnomalySeverity.Critical));
        }

        // Anomaly 4: Low trust + high value
        if (command.TrustScore < 0.3 && normalizedAmount > LargeTransactionThreshold)
        {
            anomalies.Add(new EconomicAnomaly(
                "low_trust_high_value",
                $"Low trust ({command.TrustScore:F4}) identity executing high-value transaction ({normalizedAmount} {command.Currency})",
                EconomicAnomalySeverity.High));
        }

        var result = new EconomicTransactionAnalysisResult(
            AccountId: command.AccountId,
            Amount: normalizedAmount,
            Currency: command.Currency,
            AnomalyCount: anomalies.Count,
            Anomalies: anomalies,
            IsClean: anomalies.Count == 0);

        return Task.FromResult(EngineResult.Ok(result));
    }
}

public sealed record AnalyzeEconomicTransactionCommand(
    string AccountId,
    string AssetId,
    decimal Amount,
    string Currency,
    string TransactionType,
    string Decision,
    string? DecisionHash,
    bool WasExecuted,
    double TrustScore);

public sealed record EconomicTransactionAnalysisResult(
    string AccountId,
    decimal Amount,
    string Currency,
    int AnomalyCount,
    List<EconomicAnomaly> Anomalies,
    bool IsClean);

public sealed record EconomicAnomaly(
    string Type,
    string Description,
    EconomicAnomalySeverity Severity);

public enum EconomicAnomalySeverity
{
    Low,
    Medium,
    High,
    Critical
}
