namespace Whycespace.Engines.T3I.Atlas.Service;

/// <summary>
/// T3I engine: demand forecasting business logic.
/// Extracted from Systems.WhyceAtlas.DemandForecastService.
/// </summary>
public sealed class DemandForecastEngine
{
    public DemandForecastResult Forecast(DemandForecastCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var historicalDemand = command.HistoricalDemand;

        var totalDemand = historicalDemand.Sum(d => d.Volume);
        var avgDaily = historicalDemand.Count > 0 ? totalDemand / historicalDemand.Count : 0;

        return new DemandForecastResult
        {
            ClusterId = command.ClusterId,
            ServiceType = command.ServiceType,
            PredictedDailyVolume = avgDaily,
            PredictedTotalVolume = avgDaily * command.HorizonDays,
            HorizonDays = command.HorizonDays,
            Confidence = historicalDemand.Count >= 30 ? 0.85 : 0.5
        };
    }
}

public sealed record DemandForecastCommand
{
    public required string ClusterId { get; init; }
    public required string ServiceType { get; init; }
    public required int HorizonDays { get; init; }
    public required IReadOnlyList<DemandRecord> HistoricalDemand { get; init; }
}

public sealed record DemandRecord
{
    public required string Date { get; init; }
    public required int Volume { get; init; }
}

public sealed record DemandForecastResult
{
    public required string ClusterId { get; init; }
    public required string ServiceType { get; init; }
    public required int PredictedDailyVolume { get; init; }
    public required int PredictedTotalVolume { get; init; }
    public required int HorizonDays { get; init; }
    public required double Confidence { get; init; }
}
