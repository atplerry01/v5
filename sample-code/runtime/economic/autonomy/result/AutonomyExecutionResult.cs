using Whycespace.Domain.IntelligenceSystem.Economic.Autonomy;

namespace Whycespace.Runtime.Economic.Autonomy.Result;

public sealed record AutonomyExecutionResult
{
    public required bool IsEnabled { get; init; }
    public required bool IsAllowed { get; init; }
    public AutonomousDecision? Decision { get; init; }
    public string? Reason { get; init; }

    public static AutonomyExecutionResult Disabled() =>
        new() { IsEnabled = false, IsAllowed = false, Reason = "Autonomy disabled" };

    public static AutonomyExecutionResult Blocked(string reason) =>
        new() { IsEnabled = true, IsAllowed = false, Reason = reason };

    public static AutonomyExecutionResult NoCandidates() =>
        new() { IsEnabled = true, IsAllowed = true, Reason = "No valid autonomous path within constraints" };

    public static AutonomyExecutionResult Success(AutonomousDecision decision) =>
        new() { IsEnabled = true, IsAllowed = true, Decision = decision };
}
