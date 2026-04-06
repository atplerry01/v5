namespace Whycespace.Runtime.GuardExecution.Engine;

public sealed record GuardExecutionReport
{
    public required int GuardsExecuted { get; init; }
    public required IReadOnlyList<GuardResult> Results { get; init; }
    public required GuardExecutionStatus Status { get; init; }
    public required DateTimeOffset Timestamp { get; init; }

    public IReadOnlyList<GuardViolation> AllViolations =>
        Results.SelectMany(r => r.Violations).ToList();

    public IReadOnlyList<GuardViolation> BlockingViolations =>
        AllViolations.Where(v => v.Severity is GuardSeverity.S0 or GuardSeverity.S1).ToList();

    public IReadOnlyList<GuardViolation> Warnings =>
        AllViolations.Where(v => v.Severity == GuardSeverity.S2).ToList();
}

public enum GuardExecutionStatus
{
    Pass,
    Fail,
    Warn
}
