namespace Whycespace.Runtime.Reliability;

/// <summary>
/// Runtime handler that detects system failures and initiates continuity procedures.
/// Observes engine execution results and triggers failover when thresholds are breached.
/// Runtime-only — orchestrates, does not contain business logic.
/// </summary>
public sealed class FailureDetectionHandler
{
    private readonly FailureDetectionConfig _config;

    public FailureDetectionHandler(FailureDetectionConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Evaluates an execution result and determines if a continuity trigger is needed.
    /// Returns a trigger command if thresholds are breached, null otherwise.
    /// </summary>
    public ContinuityTriggerCommand? Evaluate(ExecutionHealthSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        // Consecutive failure threshold
        if (snapshot.ConsecutiveFailures >= _config.ConsecutiveFailureThreshold)
        {
            return new ContinuityTriggerCommand(
                ClusterId: snapshot.ClusterId,
                Reason: $"Consecutive failures ({snapshot.ConsecutiveFailures}) exceeded threshold ({_config.ConsecutiveFailureThreshold})",
                TriggerType: "CONSECUTIVE_FAILURE",
                Severity: "Critical");
        }

        // Error rate threshold
        if (snapshot.TotalExecutions > 0)
        {
            var errorRate = (decimal)snapshot.FailedExecutions / snapshot.TotalExecutions;
            if (errorRate >= _config.ErrorRateThreshold)
            {
                return new ContinuityTriggerCommand(
                    ClusterId: snapshot.ClusterId,
                    Reason: $"Error rate ({errorRate:P0}) exceeded threshold ({_config.ErrorRateThreshold:P0})",
                    TriggerType: "ERROR_RATE",
                    Severity: errorRate >= 0.5m ? "Critical" : "High");
            }
        }

        // Latency threshold
        if (snapshot.P99LatencyMs > _config.LatencyThresholdMs)
        {
            return new ContinuityTriggerCommand(
                ClusterId: snapshot.ClusterId,
                Reason: $"P99 latency ({snapshot.P99LatencyMs}ms) exceeded threshold ({_config.LatencyThresholdMs}ms)",
                TriggerType: "LATENCY_DEGRADATION",
                Severity: "High");
        }

        return null;
    }
}

public sealed record ExecutionHealthSnapshot(
    Guid ClusterId,
    int ConsecutiveFailures,
    int FailedExecutions,
    int TotalExecutions,
    long P99LatencyMs);

public sealed record ContinuityTriggerCommand(
    Guid ClusterId,
    string Reason,
    string TriggerType,
    string Severity);

public sealed record FailureDetectionConfig(
    int ConsecutiveFailureThreshold = 5,
    decimal ErrorRateThreshold = 0.2m,
    long LatencyThresholdMs = 5000);
