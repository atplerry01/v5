using System.Text.RegularExpressions;
using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Analyzers;

/// <summary>
/// Builds a dependency graph from source files and validates layer direction.
/// Ensures no upward or lateral dependencies violate the architecture.
/// </summary>
public sealed partial class DependencyGraphBuilder : IGuardAnalyzer
{
    public string AnalyzerId => "DependencyGraphBuilder";

    // Layer ordering (lower index = lower layer, cannot depend on higher)
    private static readonly (string Layer, string PathPrefix)[] LayerOrder =
    [
        ("shared", "src/shared/"),
        ("domain", "src/domain/"),
        ("engines", "src/engines/"),
        ("runtime", "src/runtime/"),
        ("systems", "src/systems/"),
        ("platform", "src/platform/"),
    ];

    // Allowed exceptions (domain can use shared, engines can use domain+shared, etc.)
    private static readonly Dictionary<string, HashSet<string>> AllowedDependencies = new()
    {
        ["shared"] = [],
        ["domain"] = new() { "shared" },
        ["engines"] = new() { "shared", "domain" },
        ["runtime"] = new() { "shared", "domain" },
        ["systems"] = new() { "shared", "runtime" },
        ["platform"] = new() { "shared", "runtime", "systems" },
    };

    [GeneratedRegex(@"using\s+(Whycespace\.\w+)", RegexOptions.Compiled)]
    private static partial Regex UsingPattern();

    public Task<AnalysisResult> AnalyzeAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        if (context.Mode != GuardExecutionMode.Ci)
            return Task.FromResult(AnalysisResult.Ok(AnalyzerId));

        var findings = new List<AnalysisFinding>();

        foreach (var file in context.SourceFiles.Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
        {
            var normalized = file.Replace('\\', '/');
            var sourceLayer = ResolveLayer(normalized);
            if (sourceLayer is null) continue;

            var content = File.ReadAllText(file);
            var matches = UsingPattern().Matches(content);

            foreach (Match match in matches)
            {
                var importedNamespace = match.Groups[1].Value;
                var targetLayer = ResolveLayerFromNamespace(importedNamespace);
                if (targetLayer is null || targetLayer == sourceLayer) continue;

                if (!AllowedDependencies.TryGetValue(sourceLayer, out var allowed) || !allowed.Contains(targetLayer))
                {
                    findings.Add(new AnalysisFinding
                    {
                        Rule = "DEPENDENCY.INVALID_DIRECTION",
                        Description = $"'{sourceLayer}' layer depends on '{targetLayer}' layer via '{importedNamespace}'",
                        File = file,
                        Severity = GuardSeverity.S0
                    });
                }
            }
        }

        return Task.FromResult(AnalysisResult.WithFindings(AnalyzerId, findings));
    }

    private static string? ResolveLayer(string path)
    {
        foreach (var (layer, prefix) in LayerOrder)
        {
            if (path.Contains(prefix, StringComparison.OrdinalIgnoreCase))
                return layer;
        }
        return null;
    }

    private static string? ResolveLayerFromNamespace(string ns)
    {
        if (ns.StartsWith("Whycespace.Shared", StringComparison.Ordinal)) return "shared";
        if (ns.StartsWith("Whycespace.Domain", StringComparison.Ordinal)) return "domain";
        if (ns.StartsWith("Whycespace.Engines", StringComparison.Ordinal)) return "engines";
        if (ns.StartsWith("Whycespace.Runtime", StringComparison.Ordinal)) return "runtime";
        if (ns.StartsWith("Whycespace.Systems", StringComparison.Ordinal)) return "systems";
        if (ns.StartsWith("Whycespace.Platform", StringComparison.Ordinal)) return "platform";
        return null;
    }
}
