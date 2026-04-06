using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;
using ObservabilityAlertThreshold = Whycespace.Shared.Contracts.Observability.AlertThreshold;
using ObservabilityAlertSeverity = Whycespace.Shared.Contracts.Observability.AlertSeverity;

namespace Whycespace.Engines.T3I.Observability.Diagnostic;

/// <summary>
/// T3I engine: runs health checks and evaluates metric thresholds.
/// Returns diagnostic results as read models — does NOT write to sinks.
/// Sink routing is the responsibility of runtime middleware.
/// </summary>
public sealed class DiagnosticEngine
{
    private readonly IReadOnlyList<IHealthCheck> _healthChecks;
    private readonly IClock _clock;
    private readonly IIdGenerator _idGen;

    public DiagnosticEngine(IReadOnlyList<IHealthCheck> healthChecks, IClock clock, IIdGenerator idGen)
    {
        _healthChecks = healthChecks;
        _clock = clock;
        _idGen = idGen;
    }

    public async Task<SystemDiagnosticReport> RunDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<HealthCheckResult>();

        foreach (var check in _healthChecks)
        {
            try
            {
                var result = await check.CheckAsync(cancellationToken);
                results.Add(result);
            }
            catch (Exception ex)
            {
                results.Add(new HealthCheckResult
                {
                    Component = check.Name,
                    Status = HealthStatus.Unhealthy,
                    Message = ex.Message
                });
            }
        }

        return new SystemDiagnosticReport
        {
            Timestamp = _clock.UtcNowOffset,
            OverallStatus = results.All(r => r.Status == HealthStatus.Healthy)
                ? HealthStatus.Healthy
                : results.Any(r => r.Status == HealthStatus.Unhealthy)
                    ? HealthStatus.Unhealthy
                    : HealthStatus.Degraded,
            Results = results
        };
    }

    public IReadOnlyList<MonitoringAlert> EvaluateMetric(MetricEntry metric, IReadOnlyList<ObservabilityAlertThreshold> thresholds)
    {
        var alerts = new List<MonitoringAlert>();

        foreach (var threshold in thresholds)
        {
            if (metric.Name != threshold.MetricName)
                continue;

            var breached = threshold.Direction == ThresholdDirection.Above
                ? metric.Value > threshold.Value
                : metric.Value < threshold.Value;

            if (breached)
            {
                alerts.Add(new MonitoringAlert
                {
                    AlertId = _idGen.DeterministicGuid($"Alert:{metric.Name}:{threshold.MetricName}:{threshold.Direction}").ToString("N"),
                    MetricName = metric.Name,
                    ActualValue = metric.Value,
                    ThresholdValue = threshold.Value,
                    Severity = threshold.Severity,
                    Message = $"{metric.Name} {threshold.Direction.ToString().ToLowerInvariant()} threshold: {metric.Value} (limit: {threshold.Value})",
                    Timestamp = _clock.UtcNowOffset
                });
            }
        }

        return alerts;
    }
}
