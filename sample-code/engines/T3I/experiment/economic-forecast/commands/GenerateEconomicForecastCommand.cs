namespace Whycespace.Engines.T3I.EconomicForecast;

public sealed record GenerateEconomicForecastCommand
{
    public required string ForecastId { get; init; }
    public required string IdentityId { get; init; }
    public string? WalletId { get; init; }
    public required string Scope { get; init; }
    public required string CorrelationId { get; init; }
    public required string SourceEventId { get; init; }
    public required DateTimeOffset WindowStart { get; init; }
    public required DateTimeOffset WindowEnd { get; init; }
    public required decimal Volume { get; init; }
    public required decimal Velocity { get; init; }
    public required int TransactionCount { get; init; }
    public required decimal AverageTransactionSize { get; init; }
    public required string ForecastType { get; init; }
    public required string TimeHorizon { get; init; }
}

/// <summary>
/// Engine-local forecast type constants — decoupled from domain ForecastType.
/// </summary>
public static class ForecastTypes
{
    public const string Balance = "Balance";
    public const string Usage = "Usage";
    public const string Revenue = "Revenue";
}

/// <summary>
/// Engine-local time horizon constants — decoupled from domain TimeHorizon.
/// </summary>
public static class TimeHorizons
{
    public const string Short = "Short";
    public const string Medium = "Medium";
    public const string Long = "Long";
}
