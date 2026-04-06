namespace Whycespace.Engines.T3I.Monitoring;

public sealed class AnomalyDetectionEngine
{
    public AnomalyResult Detect(AnomalyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new AnomalyResult(command.CheckId, false, string.Empty);
    }
}

public sealed record AnomalyCommand(string CheckId, string MetricId, decimal Threshold);

public sealed record AnomalyResult(string CheckId, bool AnomalyDetected, string Description);
