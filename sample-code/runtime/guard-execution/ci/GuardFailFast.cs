using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Ci;

/// <summary>
/// Determines whether the CI pipeline should fail fast based on guard results.
/// S0 = immediate failure, S1 = fail after all guards complete.
/// </summary>
public static class GuardFailFast
{
    public static bool ShouldFailImmediately(GuardResult result) =>
        result.Violations.Any(v => v.Severity == GuardSeverity.S0);

    public static bool ShouldFail(GuardExecutionReport report) =>
        report.Status == GuardExecutionStatus.Fail;

    public static IReadOnlyList<GuardViolation> GetBlockingViolations(GuardExecutionReport report) =>
        report.BlockingViolations;

    public static int ToExitCode(GuardExecutionReport report) =>
        report.Status switch
        {
            GuardExecutionStatus.Pass => 0,
            GuardExecutionStatus.Warn => 0, // Warnings don't fail CI
            GuardExecutionStatus.Fail => 1,
            _ => 1
        };
}
