namespace Whycespace.Runtime.GuardExecution.Engine;

/// <summary>
/// Thrown when a guard detects a critical violation that must halt execution.
/// </summary>
public sealed class GuardViolationException : Exception
{
    public GuardExecutionReport Report { get; }

    public GuardViolationException(GuardExecutionReport report)
        : base(FormatMessage(report))
    {
        Report = report;
    }

    private static string FormatMessage(GuardExecutionReport report)
    {
        var first = report.BlockingViolations.FirstOrDefault();
        return first is not null
            ? $"Guard violation [{first.Rule}]: {first.Description}"
            : "Guard execution detected critical violations.";
    }
}
