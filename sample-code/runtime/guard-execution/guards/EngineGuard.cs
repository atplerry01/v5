using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Guards;

/// <summary>
/// Enforces engine tier topology:
/// - T0U-T4A tiered structure
/// - Engines import from domain, never define domain models
/// - No direct engine invocation from platform layer
/// </summary>
public sealed class EngineGuard : IGuard
{
    public string Name => "EngineGuard";
    public GuardCategory Category => GuardCategory.Engine;
    public GuardPhase Phase => GuardPhase.PrePolicy;

    private static readonly string[] EngineTiers = ["T0U", "T1M", "T2E", "T3I", "T4A"];

    public Task<GuardResult> EvaluateAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        if (context.Mode != GuardExecutionMode.Ci)
            return Task.FromResult(GuardResult.Pass(Name));

        var violations = new List<GuardViolation>();

        // Check engine files are within valid tiers
        var engineFiles = context.SourceFiles
            .Where(f => f.Replace('\\', '/').Contains("src/engines/", StringComparison.OrdinalIgnoreCase)
                        && f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));

        foreach (var file in engineFiles)
        {
            var normalized = file.Replace('\\', '/');
            var afterEngines = normalized[(normalized.IndexOf("src/engines/", StringComparison.OrdinalIgnoreCase) + "src/engines/".Length)..];
            var tierSegment = afterEngines.Split('/').FirstOrDefault() ?? "";

            if (!EngineTiers.Any(t => tierSegment.Equals(t, StringComparison.OrdinalIgnoreCase)))
            {
                violations.Add(new GuardViolation
                {
                    Rule = "ENGINE.INVALID_TIER",
                    Severity = GuardSeverity.S0,
                    File = file,
                    Description = $"Engine file not in a valid tier directory: '{tierSegment}'",
                    Expected = $"Engine files must be under one of: {string.Join(", ", EngineTiers)}",
                    Actual = $"Found under '{tierSegment}'",
                    Remediation = "Move the engine file to the appropriate tier directory."
                });
            }
        }

        // Check platform layer for direct engine references
        var platformFiles = context.SourceFiles
            .Where(f => f.Replace('\\', '/').Contains("src/platform/", StringComparison.OrdinalIgnoreCase)
                        && f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));

        foreach (var file in platformFiles)
        {
            var content = File.ReadAllText(file);
            if (content.Contains("using Whycespace.Engines", StringComparison.Ordinal))
            {
                violations.Add(new GuardViolation
                {
                    Rule = "ENGINE.DIRECT_INVOCATION",
                    Severity = GuardSeverity.S0,
                    File = file,
                    Description = "Platform layer directly references engine namespace",
                    Expected = "Platform must route through runtime control plane, never import engines directly",
                    Actual = "Found 'using Whycespace.Engines' in platform layer",
                    Remediation = "Remove engine reference. Use RuntimeControlPlane for command dispatch."
                });
            }
        }

        return Task.FromResult(violations.Count == 0
            ? GuardResult.Pass(Name)
            : GuardResult.Fail(Name, violations));
    }
}
