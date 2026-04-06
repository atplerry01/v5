using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Guards;

/// <summary>
/// Enforces runtime invariants:
/// - Commands must flow through RuntimeControlPlane
/// - No runtime bypass (direct dispatcher calls)
/// - Middleware pipeline integrity
/// </summary>
public sealed class RuntimeGuard : IGuard
{
    public string Name => "RuntimeGuard";
    public GuardCategory Category => GuardCategory.Runtime;
    public GuardPhase Phase => GuardPhase.PrePolicy;

    private static readonly string[] BypassPatterns =
    [
        "new CommandDispatcher(",
        "dispatcher.DispatchAsync(",
        "DispatchAsync(context)"
    ];

    public Task<GuardResult> EvaluateAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        var violations = new List<GuardViolation>();

        if (context.Mode == GuardExecutionMode.Ci)
        {
            // Static: detect runtime bypass in platform/systems layers
            var targetFiles = context.SourceFiles
                .Where(f =>
                {
                    var n = f.Replace('\\', '/');
                    return (n.Contains("src/platform/", StringComparison.OrdinalIgnoreCase)
                            || n.Contains("src/systems/", StringComparison.OrdinalIgnoreCase))
                           && f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase);
                });

            foreach (var file in targetFiles)
            {
                var content = File.ReadAllText(file);
                foreach (var pattern in BypassPatterns)
                {
                    if (content.Contains(pattern, StringComparison.Ordinal))
                    {
                        violations.Add(new GuardViolation
                        {
                            Rule = "RUNTIME.BYPASS_DETECTED",
                            Severity = GuardSeverity.S0,
                            File = file,
                            Description = $"Direct runtime bypass detected: '{pattern}'",
                            Expected = "All commands must flow through RuntimeControlPlane.ExecuteAsync",
                            Actual = $"Found direct dispatcher usage: '{pattern}'",
                            Remediation = "Route all commands through RuntimeControlPlane instead of calling dispatchers directly."
                        });
                    }
                }
            }
        }
        else
        {
            // Runtime: verify command has proper routing context
            if (string.IsNullOrWhiteSpace(context.CorrelationId))
            {
                violations.Add(new GuardViolation
                {
                    Rule = "RUNTIME.MISSING_CORRELATION",
                    Severity = GuardSeverity.S1,
                    File = "runtime",
                    Description = "Command is missing CorrelationId",
                    Expected = "All commands must have a CorrelationId for tracing",
                    Remediation = "Ensure CommandEnvelope includes a valid CorrelationId."
                });
            }
        }

        return Task.FromResult(violations.Count == 0
            ? GuardResult.Pass(Name)
            : GuardResult.Fail(Name, violations));
    }
}
