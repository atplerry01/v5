using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.Intelligence.Economic;

/// <summary>
/// T3I cost leakage detector. Identifies systematic over-payments,
/// fee miscalculations, and settlement inefficiencies.
/// Stateless, deterministic. Outputs leakage analysis.
/// </summary>
public sealed class CostLeakageDetector : IEngine<CostLeakageCommand>
{
    public Task<EngineResult> ExecuteAsync(
        CostLeakageCommand command, EngineContext context, CancellationToken cancellationToken = default)
    {
        var leakages = new List<CostLeakage>();

        // Fee overpayment detection
        if (command.ActualFees > command.ExpectedFees * 1.05m)
        {
            var overpayment = command.ActualFees - command.ExpectedFees;
            leakages.Add(new CostLeakage
            {
                Category = "FeeOverpayment",
                Description = $"Fees exceed expected by {overpayment:C0}",
                Amount = overpayment,
                Confidence = ConfidenceScore.High
            });
        }

        // Settlement delay cost
        if (command.AvgSettlementDelayHours > command.TargetSettlementHours)
        {
            var delayCost = command.DailyVolume * command.CostOfCapitalRate
                * (command.AvgSettlementDelayHours - command.TargetSettlementHours) / 24m;
            leakages.Add(new CostLeakage
            {
                Category = "SettlementDelay",
                Description = $"Settlement delay costing {delayCost:C0}/day in opportunity cost",
                Amount = delayCost,
                Confidence = ConfidenceScore.Medium
            });
        }

        return Task.FromResult(EngineResult.Ok(new CostLeakageResult
        {
            Leakages = leakages,
            TotalLeakage = leakages.Sum(l => l.Amount)
        }));
    }
}

public sealed record CostLeakageCommand
{
    public required decimal ActualFees { get; init; }
    public required decimal ExpectedFees { get; init; }
    public required decimal AvgSettlementDelayHours { get; init; }
    public required decimal TargetSettlementHours { get; init; }
    public required decimal DailyVolume { get; init; }
    public required decimal CostOfCapitalRate { get; init; }
    public required string CorrelationId { get; init; }
}

public sealed record CostLeakage
{
    public required string Category { get; init; }
    public required string Description { get; init; }
    public required decimal Amount { get; init; }
    public required ConfidenceScore Confidence { get; init; }
}

public sealed record CostLeakageResult
{
    public required IReadOnlyList<CostLeakage> Leakages { get; init; }
    public required decimal TotalLeakage { get; init; }
}
