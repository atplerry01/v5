namespace Whycespace.Systems.Midstream.WhyceAtlas.Simulation;

public sealed class SimulationEvaluator
{
    public SimulationEvaluation Evaluate(SimulationSandbox sandbox)
    {
        var executions = sandbox.Executions;

        var policyChecks = executions.Count(e => e.CommandType.StartsWith("policy.", StringComparison.OrdinalIgnoreCase));
        var engineCalls = executions.Count(e => e.CommandType.StartsWith("engine.", StringComparison.OrdinalIgnoreCase));
        var auditLogs = executions.Count(e => e.CommandType.StartsWith("whycechain.", StringComparison.OrdinalIgnoreCase));
        var eventEmissions = executions.Count(e => e.CommandType.StartsWith("event.", StringComparison.OrdinalIgnoreCase));

        var hasPolicyGate = policyChecks > 0;
        var hasAuditTrail = auditLogs > 0;

        var verdict = (hasPolicyGate, hasAuditTrail) switch
        {
            (true, true) => SimulationVerdict.Pass,
            (true, false) => SimulationVerdict.Risk,
            (false, _) => SimulationVerdict.Fail
        };

        return new SimulationEvaluation
        {
            Verdict = verdict,
            TotalSteps = executions.Count,
            PolicyChecks = policyChecks,
            EngineCalls = engineCalls,
            AuditLogs = auditLogs,
            EventEmissions = eventEmissions,
            Executions = executions,
            Reason = verdict switch
            {
                SimulationVerdict.Pass => "All checks passed: policy-gated, audited.",
                SimulationVerdict.Risk => "Missing audit trail — execution may proceed with caution.",
                SimulationVerdict.Fail => "No policy gate detected — execution blocked.",
                _ => "Unknown"
            }
        };
    }
}

public sealed record SimulationEvaluation
{
    public required SimulationVerdict Verdict { get; init; }
    public required int TotalSteps { get; init; }
    public required int PolicyChecks { get; init; }
    public required int EngineCalls { get; init; }
    public required int AuditLogs { get; init; }
    public required int EventEmissions { get; init; }
    public required IReadOnlyList<SimulatedExecution> Executions { get; init; }
    public required string Reason { get; init; }
}

public enum SimulationVerdict
{
    Pass,
    Risk,
    Fail
}
