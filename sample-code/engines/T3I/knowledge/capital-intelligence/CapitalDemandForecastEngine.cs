namespace Whycespace.Engines.T3I.CapitalIntelligence;

/// <summary>
/// T3I Capital Demand Forecast Engine -- projects capital demand over a time horizon.
///
/// Forecasts:
///   - Projected demand based on current outflow velocity
///   - Surplus/deficit relative to available capital
///   - Risk level assessment based on projected surplus
///   - Confidence scoring that degrades over longer horizons
///
/// Stateless. No persistence. Pure computation only.
/// No domain imports -- uses engine-local types only.
/// </summary>
public sealed class CapitalDemandForecastEngine
{
    public CapitalForecastResult Forecast(ForecastCapitalDemandCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var horizonMultiplier = command.HorizonDays switch
        {
            <= 7 => 1.0m,
            <= 30 => 0.85m,
            <= 90 => 0.7m,
            _ => 0.5m
        };

        var confidence = command.HorizonDays switch
        {
            <= 7 => 0.9m,
            <= 30 => 0.75m,
            <= 90 => 0.6m,
            _ => 0.45m
        };

        // Project demand based on current velocity and commitments
        var dailyBurnRate = command.CurrentOutflow / Math.Max(command.ObservationWindowDays, 1);
        var projectedDemand = dailyBurnRate * command.HorizonDays * horizonMultiplier;
        var projectedWithCommitments = projectedDemand + command.PendingCommitments;

        var surplus = command.AvailableCapital - projectedWithCommitments;
        var riskLevel = surplus switch
        {
            < 0 => "Critical",
            var s when s < projectedWithCommitments * 0.1m => "High",
            var s when s < projectedWithCommitments * 0.25m => "Medium",
            _ => "Low"
        };

        return new CapitalForecastResult(
            ProjectedDemand: projectedWithCommitments,
            Surplus: surplus,
            RiskLevel: riskLevel,
            Confidence: confidence,
            HorizonDays: command.HorizonDays);
    }
}

// -- Commands --

public sealed record ForecastCapitalDemandCommand(
    decimal CurrentOutflow,
    int ObservationWindowDays,
    int HorizonDays,
    decimal PendingCommitments,
    decimal AvailableCapital);

// -- Results --

public sealed record CapitalForecastResult(
    decimal ProjectedDemand,
    decimal Surplus,
    string RiskLevel,
    decimal Confidence,
    int HorizonDays);
