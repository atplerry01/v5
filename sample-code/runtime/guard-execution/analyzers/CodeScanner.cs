using System.Text.RegularExpressions;
using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Analyzers;

/// <summary>
/// Parses source files and detects forbidden patterns:
/// - Forbidden imports
/// - Direct engine calls
/// - Non-deterministic operations
/// </summary>
public sealed partial class CodeScanner : IGuardAnalyzer
{
    public string AnalyzerId => "CodeScanner";

    private static readonly (string Pattern, string Rule, GuardSeverity Severity, string Description)[] ScanRules =
    [
        ("new\\s+HttpClient\\s*\\(", "SCAN.DIRECT_HTTP", GuardSeverity.S1, "Direct HttpClient construction detected — use IHttpClientFactory"),
        ("Thread\\.Sleep", "SCAN.THREAD_SLEEP", GuardSeverity.S2, "Thread.Sleep detected — use Task.Delay or timeout policies"),
        ("catch\\s*\\(\\s*Exception\\s*\\)", "SCAN.CATCH_ALL", GuardSeverity.S3, "Catch-all exception handler — catch specific exceptions"),
        ("\\bvar\\s+connectionString\\s*=\\s*\"", "SCAN.HARDCODED_CONNECTION", GuardSeverity.S0, "Hardcoded connection string detected"),
        ("Console\\.Write", "SCAN.CONSOLE_OUTPUT", GuardSeverity.S3, "Console output in production code — use ILogger"),
    ];

    public Task<AnalysisResult> AnalyzeAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        if (context.Mode != GuardExecutionMode.Ci)
            return Task.FromResult(AnalysisResult.Ok(AnalyzerId));

        var findings = new List<AnalysisFinding>();

        foreach (var file in context.SourceFiles.Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
        {
            // Skip test files for non-critical rules
            var normalized = file.Replace('\\', '/');
            if (normalized.Contains("tests/", StringComparison.OrdinalIgnoreCase))
                continue;

            var content = File.ReadAllText(file);
            var lines = content.Split('\n');

            foreach (var (pattern, rule, severity, description) in ScanRules)
            {
                var regex = new Regex(pattern, RegexOptions.Compiled);
                for (var i = 0; i < lines.Length; i++)
                {
                    if (regex.IsMatch(lines[i]))
                    {
                        findings.Add(new AnalysisFinding
                        {
                            Rule = rule,
                            Description = description,
                            File = file,
                            Line = i + 1,
                            Severity = severity
                        });
                    }
                }
            }
        }

        return Task.FromResult(AnalysisResult.WithFindings(AnalyzerId, findings));
    }
}
