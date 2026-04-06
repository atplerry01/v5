using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Contracts;

public interface IGuardAnalyzer
{
    string AnalyzerId { get; }
    Task<AnalysisResult> AnalyzeAsync(GuardContext context, CancellationToken cancellationToken = default);
}

public sealed record AnalysisResult
{
    public required string AnalyzerId { get; init; }
    public required bool Success { get; init; }
    public IReadOnlyList<AnalysisFinding> Findings { get; init; } = [];
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();

    public static AnalysisResult Ok(string analyzerId) =>
        new() { AnalyzerId = analyzerId, Success = true };

    public static AnalysisResult WithFindings(string analyzerId, IReadOnlyList<AnalysisFinding> findings) =>
        new() { AnalyzerId = analyzerId, Success = findings.Count == 0, Findings = findings };
}

public sealed record AnalysisFinding
{
    public required string Rule { get; init; }
    public required string Description { get; init; }
    public required string File { get; init; }
    public int? Line { get; init; }
    public GuardSeverity Severity { get; init; } = GuardSeverity.S1;
}
