namespace Whycespace.Domain.IntelligenceSystem.Economic.Forecast;

public sealed record ForecastType(string Value)
{
    public static readonly ForecastType Balance = new("balance");
    public static readonly ForecastType Usage = new("usage");
    public static readonly ForecastType Revenue = new("revenue");
}
