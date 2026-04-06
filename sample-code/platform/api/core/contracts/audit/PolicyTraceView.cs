namespace Whycespace.Platform.Api.Core.Contracts.Audit;

/// <summary>
/// Read-only policy evaluation trace.
/// Summarizes the policy decision made during execution.
/// No raw policy rules exposed — only decision, ID, and violations.
/// </summary>
public sealed record PolicyTraceView
{
    public required string Decision { get; init; }
    public required string PolicyId { get; init; }
    public required IReadOnlyList<string> Violations { get; init; }
    public DateTimeOffset? EvaluatedAt { get; init; }
}
