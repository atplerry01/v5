namespace Whycespace.Runtime.GuardExecution.Engine;

public sealed record GuardViolation
{
    public required string Rule { get; init; }
    public required GuardSeverity Severity { get; init; }
    public required string File { get; init; }
    public required string Description { get; init; }
    public string? Expected { get; init; }
    public string? Actual { get; init; }
    public string? Remediation { get; init; }
}
