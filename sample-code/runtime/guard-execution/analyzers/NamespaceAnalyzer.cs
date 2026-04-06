using System.Text.RegularExpressions;
using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Analyzers;

/// <summary>
/// Validates namespace conventions match physical file locations.
/// Ensures namespace alignment with the WBSM v3 layer topology.
/// </summary>
public sealed partial class NamespaceAnalyzer : IGuardAnalyzer
{
    public string AnalyzerId => "NamespaceAnalyzer";

    [GeneratedRegex(@"^namespace\s+([\w.]+)\s*;", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex NamespaceDeclaration();

    private static readonly Dictionary<string, string> PathToNamespacePrefix = new()
    {
        ["src/shared/"] = "Whycespace.Shared",
        ["src/domain/"] = "Whycespace.Domain",
        ["src/engines/"] = "Whycespace.Engines",
        ["src/runtime/"] = "Whycespace.Runtime",
        ["src/systems/"] = "Whycespace.Systems",
        ["src/platform/"] = "Whycespace.Platform",
        ["infrastructure/"] = "Whycespace.Infrastructure",
    };

    public Task<AnalysisResult> AnalyzeAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        if (context.Mode != GuardExecutionMode.Ci)
            return Task.FromResult(AnalysisResult.Ok(AnalyzerId));

        var findings = new List<AnalysisFinding>();

        foreach (var file in context.SourceFiles.Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
        {
            var content = File.ReadAllText(file);
            var match = NamespaceDeclaration().Match(content);
            if (!match.Success) continue;

            var declaredNamespace = match.Groups[1].Value;
            var normalized = file.Replace('\\', '/');

            foreach (var (pathPrefix, nsPrefix) in PathToNamespacePrefix)
            {
                if (normalized.Contains(pathPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (!declaredNamespace.StartsWith(nsPrefix, StringComparison.Ordinal))
                    {
                        findings.Add(new AnalysisFinding
                        {
                            Rule = "NAMESPACE.MISALIGNED",
                            Description = $"Namespace '{declaredNamespace}' does not match expected prefix '{nsPrefix}' for path '{pathPrefix}'",
                            File = file,
                            Severity = GuardSeverity.S2
                        });
                    }
                    break;
                }
            }
        }

        return Task.FromResult(AnalysisResult.WithFindings(AnalyzerId, findings));
    }
}
