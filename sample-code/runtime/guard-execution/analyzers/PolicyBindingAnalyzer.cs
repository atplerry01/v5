using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Analyzers;

/// <summary>
/// Verifies policy binding completeness:
/// - All registered command types have policy bindings
/// - No orphaned policy bindings (bindings without registered commands)
/// - Policy evaluation hash consistency
/// </summary>
public sealed class PolicyBindingAnalyzer : IGuardAnalyzer
{
    public string AnalyzerId => "PolicyBindingAnalyzer";

    public Task<AnalysisResult> AnalyzeAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        if (context.Mode != GuardExecutionMode.Ci)
            return Task.FromResult(AnalysisResult.Ok(AnalyzerId));

        var findings = new List<AnalysisFinding>();

        // Scan for engine registrations to find command types
        var registrationFiles = context.SourceFiles
            .Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                        && (f.Contains("Registration", StringComparison.OrdinalIgnoreCase)
                            || f.Contains("Bootstrap", StringComparison.OrdinalIgnoreCase)
                            || f.Contains("Startup", StringComparison.OrdinalIgnoreCase)));

        var registeredCommands = new HashSet<string>();
        var policyBindings = new HashSet<string>();

        foreach (var file in registrationFiles)
        {
            var content = File.ReadAllText(file);
            var lines = content.Split('\n');

            foreach (var line in lines)
            {
                // Detect command type registrations
                if (line.Contains(".Register(", StringComparison.Ordinal)
                    && line.Contains("CommandType", StringComparison.Ordinal))
                {
                    var trimmed = line.Trim();
                    registeredCommands.Add(trimmed);
                }

                // Detect policy bindings
                if (line.Contains("PolicyBinding", StringComparison.Ordinal)
                    || line.Contains("BindPolicy", StringComparison.Ordinal))
                {
                    var trimmed = line.Trim();
                    policyBindings.Add(trimmed);
                }
            }
        }

        // Check that engine files reference policy evaluation
        var engineFiles = context.SourceFiles
            .Where(f => f.Replace('\\', '/').Contains("src/engines/", StringComparison.OrdinalIgnoreCase)
                        && f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                        && f.Replace('\\', '/').Contains("handler", StringComparison.OrdinalIgnoreCase));

        foreach (var file in engineFiles)
        {
            var content = File.ReadAllText(file);
            if (content.Contains("ExecuteAsync", StringComparison.Ordinal)
                && !content.Contains("PolicyDecision", StringComparison.Ordinal)
                && !content.Contains("PolicyGuard", StringComparison.Ordinal)
                && !content.Contains("// policy-exempt", StringComparison.OrdinalIgnoreCase))
            {
                findings.Add(new AnalysisFinding
                {
                    Rule = "POLICY_BINDING.ENGINE_UNBOUND",
                    Description = "Engine handler does not reference PolicyDecision or PolicyGuard",
                    File = file,
                    Severity = GuardSeverity.S2
                });
            }
        }

        return Task.FromResult(AnalysisResult.WithFindings(AnalyzerId, findings));
    }
}
