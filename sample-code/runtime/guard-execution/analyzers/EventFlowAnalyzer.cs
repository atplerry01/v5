using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Analyzers;

/// <summary>
/// Ensures event-first design:
/// - Engine handlers must emit events
/// - No state mutation without event emission
/// - Event types must follow naming conventions
/// </summary>
public sealed class EventFlowAnalyzer : IGuardAnalyzer
{
    public string AnalyzerId => "EventFlowAnalyzer";

    public Task<AnalysisResult> AnalyzeAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        if (context.Mode != GuardExecutionMode.Ci)
            return Task.FromResult(AnalysisResult.Ok(AnalyzerId));

        var findings = new List<AnalysisFinding>();

        // Scan engine handler files for event emission
        var engineHandlerFiles = context.SourceFiles
            .Where(f =>
            {
                var n = f.Replace('\\', '/');
                return n.Contains("src/engines/", StringComparison.OrdinalIgnoreCase)
                       && f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                       && (n.Contains("handler", StringComparison.OrdinalIgnoreCase)
                           || n.Contains("Handler", StringComparison.Ordinal));
            });

        foreach (var file in engineHandlerFiles)
        {
            var content = File.ReadAllText(file);

            // Engine handlers that process commands should emit events
            if (content.Contains("ExecuteAsync", StringComparison.Ordinal)
                && !content.Contains("Publish", StringComparison.Ordinal)
                && !content.Contains("Emit", StringComparison.Ordinal)
                && !content.Contains("Apply(", StringComparison.Ordinal)
                && !content.Contains("RaiseEvent", StringComparison.Ordinal))
            {
                findings.Add(new AnalysisFinding
                {
                    Rule = "EVENT_FLOW.NO_EVENT_EMISSION",
                    Description = "Engine handler executes commands but does not emit events",
                    File = file,
                    Severity = GuardSeverity.S1
                });
            }
        }

        // Check event naming convention
        var eventFiles = context.SourceFiles
            .Where(f => f.Replace('\\', '/').Contains("event", StringComparison.OrdinalIgnoreCase)
                        && f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));

        foreach (var file in eventFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (fileName.Contains("Event", StringComparison.Ordinal)
                && !fileName.EndsWith("Event") && !fileName.EndsWith("Events")
                && !fileName.Contains("Handler") && !fileName.Contains("Publisher")
                && !fileName.Contains("Store") && !fileName.Contains("Fabric"))
            {
                findings.Add(new AnalysisFinding
                {
                    Rule = "EVENT_FLOW.NAMING_CONVENTION",
                    Description = $"Event type '{fileName}' should end with 'Event' suffix",
                    File = file,
                    Severity = GuardSeverity.S3
                });
            }
        }

        return Task.FromResult(AnalysisResult.WithFindings(AnalyzerId, findings));
    }
}
