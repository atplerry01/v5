namespace Whycespace.Engines.T3I.Atlas.Service;

/// <summary>
/// T3I engine: revenue projection business logic.
/// Extracted from Systems.WhyceAtlas.RevenueProjectionService.
/// </summary>
public sealed class RevenueProjectionEngine
{
    public RevenueProjectionResult Project(RevenueProjectionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var historicalRevenue = command.HistoricalRevenue;

        var totalRevenue = historicalRevenue.Sum(r => r.Amount);
        var avgDaily = historicalRevenue.Count > 0 ? totalRevenue / historicalRevenue.Count : 0m;

        return new RevenueProjectionResult
        {
            ClusterId = command.ClusterId,
            SpvId = command.SpvId,
            ProjectedDailyRevenue = avgDaily,
            ProjectedTotalRevenue = avgDaily * command.HorizonDays,
            HorizonDays = command.HorizonDays
        };
    }
}

public sealed record RevenueProjectionCommand
{
    public required string ClusterId { get; init; }
    public required string SpvId { get; init; }
    public required int HorizonDays { get; init; }
    public required IReadOnlyList<RevenueRecord> HistoricalRevenue { get; init; }
}

public sealed record RevenueRecord
{
    public required string Date { get; init; }
    public required decimal Amount { get; init; }
}

public sealed record RevenueProjectionResult
{
    public required string ClusterId { get; init; }
    public required string SpvId { get; init; }
    public required decimal ProjectedDailyRevenue { get; init; }
    public required decimal ProjectedTotalRevenue { get; init; }
    public required int HorizonDays { get; init; }
}
