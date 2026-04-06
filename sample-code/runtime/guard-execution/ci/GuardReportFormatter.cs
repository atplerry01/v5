using System.Text;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Ci;

/// <summary>
/// Produces structured violation reports for CI output.
/// </summary>
public sealed class GuardReportFormatter
{
    public string Format(GuardExecutionReport report)
    {
        var sb = new StringBuilder();

        sb.AppendLine("GUARD_EXECUTION_REPORT");
        sb.AppendLine("----------------------");
        sb.AppendLine($"timestamp: {report.Timestamp:O}");
        sb.AppendLine($"guards_executed: {report.GuardsExecuted}");
        sb.AppendLine($"total_violations: {report.AllViolations.Count}");
        sb.AppendLine($"blocking_violations: {report.BlockingViolations.Count}");
        sb.AppendLine($"warnings: {report.Warnings.Count}");

        if (report.AllViolations.Count > 0)
        {
            sb.AppendLine("violations:");
            foreach (var violation in report.AllViolations)
            {
                sb.AppendLine($"  - rule: {violation.Rule}");
                sb.AppendLine($"    severity: {violation.Severity}");
                sb.AppendLine($"    file: {violation.File}");
                sb.AppendLine($"    description: {violation.Description}");
                if (violation.Expected is not null)
                    sb.AppendLine($"    expected: {violation.Expected}");
                if (violation.Actual is not null)
                    sb.AppendLine($"    actual: {violation.Actual}");
                if (violation.Remediation is not null)
                    sb.AppendLine($"    remediation: {violation.Remediation}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("guard_results:");
        foreach (var result in report.Results)
        {
            sb.AppendLine($"  - guard: {result.GuardName}");
            sb.AppendLine($"    status: {(result.Passed ? "PASS" : "FAIL")}");
            sb.AppendLine($"    violations: {result.Violations.Count}");
            sb.AppendLine($"    hash: {result.GuardHash}");
        }

        sb.AppendLine($"status: {report.Status.ToString().ToUpperInvariant()}");

        return sb.ToString();
    }

    public string FormatCompact(GuardExecutionReport report)
    {
        var sb = new StringBuilder();
        sb.Append($"[GEE] {report.Status.ToString().ToUpperInvariant()} | ");
        sb.Append($"guards={report.GuardsExecuted} | ");
        sb.Append($"violations={report.AllViolations.Count} | ");
        sb.Append($"blocking={report.BlockingViolations.Count}");
        return sb.ToString();
    }
}
