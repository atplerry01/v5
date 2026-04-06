using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Guards;

/// <summary>
/// Enforces systems layer rules:
/// - Systems = composition only
/// - No execution, no domain mutation
/// - No direct engine invocation
/// </summary>
public sealed class SystemsGuard : IGuard
{
    public string Name => "SystemsGuard";
    public GuardCategory Category => GuardCategory.Systems;
    public GuardPhase Phase => GuardPhase.PrePolicy;

    private static readonly string[] ForbiddenInSystems =
    [
        "using Whycespace.Engines",
        "IEngine",
        ".ExecuteAsync(",
        "DbContext",
        "IRepository"
    ];

    public Task<GuardResult> EvaluateAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        if (context.Mode != GuardExecutionMode.Ci)
            return Task.FromResult(GuardResult.Pass(Name));

        var violations = new List<GuardViolation>();
        var systemsFiles = context.SourceFiles
            .Where(f => f.Replace('\\', '/').Contains("src/systems/", StringComparison.OrdinalIgnoreCase)
                        && f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));

        foreach (var file in systemsFiles)
        {
            var content = File.ReadAllText(file);

            foreach (var pattern in ForbiddenInSystems)
            {
                if (content.Contains(pattern, StringComparison.Ordinal))
                {
                    violations.Add(new GuardViolation
                    {
                        Rule = "SYSTEMS.EXECUTION_LEAK",
                        Severity = GuardSeverity.S0,
                        File = file,
                        Description = $"Systems layer contains execution or engine reference: '{pattern}'",
                        Expected = "Systems layer is composition-only. No execution, no domain mutation.",
                        Actual = $"Found '{pattern}'",
                        Remediation = "Remove execution logic. Systems compose commands and route to RuntimeControlPlane."
                    });
                }
            }
        }

        return Task.FromResult(violations.Count == 0
            ? GuardResult.Pass(Name)
            : GuardResult.Fail(Name, violations));
    }
}
