namespace Whycespace.Engines.T3I.Forecasting;

public sealed class ForecastEngine
{
    public ForecastResult Forecast(ForecastCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new ForecastResult(command.ForecastId, true, 0m);
    }
}

public sealed record ForecastCommand(string ForecastId, string MetricId, int HorizonDays);

public sealed record ForecastResult(string ForecastId, bool Success, decimal PredictedValue);
