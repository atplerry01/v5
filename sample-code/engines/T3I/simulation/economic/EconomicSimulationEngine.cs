using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Utils;

namespace Whycespace.Engines.T3I.Simulation;

/// <summary>
/// T3I engine: simulates economic impact of a transaction.
/// Predicts balance impact, capital flow, and threshold violations.
/// Stateless, deterministic. NEVER writes to chain or mutates state.
/// </summary>
public sealed class EconomicSimulationEngine : IEngine<SimulateEconomicImpactCommand>
{
    private const decimal LargeTransactionThreshold = 100_000m;

    public Task<EngineResult> ExecuteAsync(
        SimulateEconomicImpactCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default)
    {
        var normalizedAmount = AmountNormalizer.Normalize(command.Amount, command.Currency);

        var balanceImpact = command.Action.Contains("debit", StringComparison.OrdinalIgnoreCase)
            || command.Action.Contains("withdraw", StringComparison.OrdinalIgnoreCase)
                ? -normalizedAmount
                : normalizedAmount;

        var thresholdViolation = normalizedAmount > LargeTransactionThreshold;
        var severity = thresholdViolation
            ? (normalizedAmount > 1_000_000m ? "critical" : "high")
            : "low";

        var result = new EconomicSimulationResult(
            AccountId: command.AccountId,
            PredictedBalanceImpact: balanceImpact,
            Amount: normalizedAmount,
            Currency: command.Currency,
            ThresholdViolation: thresholdViolation,
            Severity: severity,
            PolicyAllowed: command.PolicyAllowed,
            Explanation: thresholdViolation
                ? $"Transaction of {normalizedAmount} {command.Currency} exceeds threshold of {LargeTransactionThreshold}"
                : $"Transaction of {normalizedAmount} {command.Currency} within normal limits");

        return Task.FromResult(EngineResult.Ok(result));
    }
}

public sealed record SimulateEconomicImpactCommand(
    string AccountId,
    decimal Amount,
    string Currency,
    string Action,
    bool PolicyAllowed);

public sealed record EconomicSimulationResult(
    string AccountId,
    decimal PredictedBalanceImpact,
    decimal Amount,
    string Currency,
    bool ThresholdViolation,
    string Severity,
    bool PolicyAllowed,
    string Explanation);
