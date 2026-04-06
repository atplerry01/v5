namespace Whycespace.Engines.T3I;

/// <summary>
/// Aggregates alerts from multiple intelligence engines into a unified risk picture.
/// Stateless, deterministic — no persistence, no external calls.
/// </summary>
public sealed class IntelligenceAlertAggregator
{
    public AggregatedIntelligence Aggregate(IReadOnlyList<IntelligenceAlert> alerts)
    {
        ArgumentNullException.ThrowIfNull(alerts);

        if (alerts.Count == 0)
            return AggregatedIntelligence.Clean;

        var maxSeverity = alerts.Max(a => a.Severity);
        var avgConfidence = new ConfidenceScore(alerts.Average(a => a.Confidence.Value));
        var criticalCount = alerts.Count(a => a.Severity >= AlertSeverity.Critical);
        var highCount = alerts.Count(a => a.Severity >= AlertSeverity.High);

        var overallRisk = (maxSeverity, criticalCount, highCount) switch
        {
            (AlertSeverity.Critical, > 2, _) => "EMERGENCY",
            (AlertSeverity.Critical, _, _) => "CRITICAL",
            (AlertSeverity.High, _, > 3) => "ELEVATED",
            (AlertSeverity.High, _, _) => "WARNING",
            _ => "NORMAL"
        };

        return new AggregatedIntelligence(
            OverallRisk: overallRisk,
            MaxSeverity: maxSeverity,
            AverageConfidence: avgConfidence,
            TotalAlerts: alerts.Count,
            CriticalAlerts: criticalCount,
            HighAlerts: highCount,
            Alerts: alerts);
    }
}

public sealed record AggregatedIntelligence(
    string OverallRisk,
    AlertSeverity MaxSeverity,
    ConfidenceScore AverageConfidence,
    int TotalAlerts,
    int CriticalAlerts,
    int HighAlerts,
    IReadOnlyList<IntelligenceAlert> Alerts)
{
    public static AggregatedIntelligence Clean => new(
        "NORMAL", AlertSeverity.Info, ConfidenceScore.High,
        0, 0, 0, Array.Empty<IntelligenceAlert>());

    public bool RequiresImmediate => OverallRisk is "EMERGENCY" or "CRITICAL";
}
