namespace Whycespace.Runtime.Observability;

/// <summary>
/// Enforcement-specific metrics facade over MetricsCollector.
/// All methods are non-blocking, non-throwing — safe for hot-path use.
///
/// Delegates to MetricsCollector for storage. Adds enforcement-specific
/// well-known metric names and tag conventions.
/// </summary>
public sealed class EnforcementMetrics
{
    private readonly MetricsCollector _collector;

    public EnforcementMetrics(MetricsCollector collector)
    {
        _collector = collector ?? throw new ArgumentNullException(nameof(collector));
    }

    public void RecordGuardExecution(string guardName, string phase, bool passed, string? severity = null)
    {
        try
        {
            var tags = new Dictionary<string, string>
            {
                ["guard_name"] = guardName,
                ["phase"] = phase,
                ["passed"] = passed.ToString().ToLowerInvariant()
            };
            if (severity is not null)
                tags["severity"] = severity;

            _collector.Increment(passed ? Names.GuardPassed : Names.GuardFailed, tags);
        }
        catch
        {
            // Non-throwing — observability must never block execution
        }
    }

    public void RecordPolicyDecision(string result, string commandType)
    {
        try
        {
            _collector.Increment(Names.PolicyDecision, new Dictionary<string, string>
            {
                ["result"] = result,
                ["command_type"] = commandType
            });
        }
        catch
        {
            // Non-throwing
        }
    }

    public void RecordChainAnchor(bool success, string? shardId = null)
    {
        try
        {
            var tags = new Dictionary<string, string>
            {
                ["success"] = success.ToString().ToLowerInvariant()
            };
            if (shardId is not null)
                tags["shard_id"] = shardId;

            _collector.Increment(success ? Names.ChainAnchorSuccess : Names.ChainAnchorFailure, tags);
        }
        catch
        {
            // Non-throwing
        }
    }

    public void RecordEnforcementLatency(string stage, TimeSpan duration)
    {
        try
        {
            _collector.RecordDuration(Names.EnforcementLatency, duration, new Dictionary<string, string>
            {
                ["stage"] = stage
            });
        }
        catch
        {
            // Non-throwing
        }
    }

    public void RecordEnforcementOutcome(string outcome, string commandType, int partitionId)
    {
        try
        {
            _collector.Increment(Names.EnforcementOutcome, new Dictionary<string, string>
            {
                ["outcome"] = outcome,
                ["command_type"] = commandType,
                ["partition_id"] = partitionId.ToString()
            });
        }
        catch
        {
            // Non-throwing
        }
    }

    public static class Names
    {
        public const string GuardPassed = "runtime.enforcement.guard.passed";
        public const string GuardFailed = "runtime.enforcement.guard.failed";
        public const string PolicyDecision = "runtime.enforcement.policy.decision";
        public const string ChainAnchorSuccess = "runtime.enforcement.chain.anchor.success";
        public const string ChainAnchorFailure = "runtime.enforcement.chain.anchor.failure";
        public const string EnforcementLatency = "runtime.enforcement.latency_ms";
        public const string EnforcementOutcome = "runtime.enforcement.outcome";
        public const string AnomalyEmitted = "runtime.enforcement.anomaly.emitted";
    }
}
