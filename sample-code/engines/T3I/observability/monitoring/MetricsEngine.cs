namespace Whycespace.Engines.T3I.Monitoring;

public sealed class MetricsEngine
{
    public MetricsResult Collect(MetricsCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new MetricsResult(command.MetricId, true, 0m);
    }
}

public sealed record MetricsCommand(string MetricId, string MetricName);

public sealed record MetricsResult(string MetricId, bool Success, decimal Value);
