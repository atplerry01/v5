using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Guards;

/// <summary>
/// Enforces WBSM v3 structural topology:
/// - Domain has zero external dependencies
/// - Shared contains no business logic
/// - Engines import from domain, never define domain models
/// - Infrastructure is adapters only
/// </summary>
public sealed class StructuralGuard : IGuard
{
    public string Name => "StructuralGuard";
    public GuardCategory Category => GuardCategory.Structural;
    public GuardPhase Phase => GuardPhase.PrePolicy;

    private static readonly IReadOnlyDictionary<string, string[]> ForbiddenImports = new Dictionary<string, string[]>
    {
        ["src/domain/"] = ["Whycespace.Runtime", "Whycespace.Infrastructure", "Whycespace.Platform", "Whycespace.Systems", "Whycespace.Engines"],
        ["src/shared/"] = ["Whycespace.Domain", "Whycespace.Runtime", "Whycespace.Infrastructure", "Whycespace.Platform"],
        ["infrastructure/"] = ["Whycespace.Domain", "Whycespace.Runtime", "Whycespace.Engines"],
    };

    public Task<GuardResult> EvaluateAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        if (context.Mode != GuardExecutionMode.Ci)
            return Task.FromResult(GuardResult.Pass(Name));

        var violations = new List<GuardViolation>();

        foreach (var file in context.SourceFiles.Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
        {
            foreach (var (layerPath, forbidden) in ForbiddenImports)
            {
                if (!file.Replace('\\', '/').Contains(layerPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                var content = File.ReadAllText(file);
                foreach (var ns in forbidden)
                {
                    if (content.Contains($"using {ns}", StringComparison.Ordinal))
                    {
                        violations.Add(new GuardViolation
                        {
                            Rule = "STRUCTURAL.FORBIDDEN_IMPORT",
                            Severity = GuardSeverity.S0,
                            File = file,
                            Description = $"Layer violation: '{layerPath}' must not import '{ns}'",
                            Expected = "No cross-layer dependency violations",
                            Actual = $"Found 'using {ns}' in {layerPath} layer",
                            Remediation = $"Remove the import from '{ns}'. Domain must have zero external dependencies."
                        });
                    }
                }
            }
        }

        return Task.FromResult(violations.Count == 0
            ? GuardResult.Pass(Name)
            : GuardResult.Fail(Name, violations));
    }
}
