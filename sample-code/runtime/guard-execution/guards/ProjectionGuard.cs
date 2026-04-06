using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Guards;

/// <summary>
/// Enforces projection rules:
/// - Projections are read-only (no command dispatch)
/// - Projections must live in runtime/projection layer
/// - No domain mutation from projection handlers
/// </summary>
public sealed class ProjectionGuard : IGuard
{
    public string Name => "ProjectionGuard";
    public GuardCategory Category => GuardCategory.Projection;
    public GuardPhase Phase => GuardPhase.PostPolicy;

    private static readonly string[] ForbiddenInProjections =
    [
        "ExecuteAsync(",
        "DispatchAsync(",
        "CommandEnvelope",
        ".Save(",
        ".Update(",
        ".Delete("
    ];

    public Task<GuardResult> EvaluateAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        if (context.Mode != GuardExecutionMode.Ci)
            return Task.FromResult(GuardResult.Pass(Name));

        var violations = new List<GuardViolation>();
        var projectionFiles = context.SourceFiles
            .Where(f =>
            {
                var n = f.Replace('\\', '/');
                return n.Contains("projection", StringComparison.OrdinalIgnoreCase)
                       && f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                       && !n.Contains("ProjectionStore", StringComparison.OrdinalIgnoreCase);
            });

        foreach (var file in projectionFiles)
        {
            var content = File.ReadAllText(file);

            foreach (var pattern in ForbiddenInProjections)
            {
                if (content.Contains(pattern, StringComparison.Ordinal)
                    && !content.Contains("// guard-exempt", StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add(new GuardViolation
                    {
                        Rule = "PROJECTION.MUTATION_DETECTED",
                        Severity = GuardSeverity.S1,
                        File = file,
                        Description = $"Potential mutation in projection: '{pattern}'",
                        Expected = "Projections must be read-only event handlers",
                        Actual = $"Found '{pattern}' in projection file",
                        Remediation = "Projections should only read events and update read models, never dispatch commands."
                    });
                }
            }
        }

        return Task.FromResult(violations.Count == 0
            ? GuardResult.Pass(Name)
            : GuardResult.Fail(Name, violations));
    }
}
