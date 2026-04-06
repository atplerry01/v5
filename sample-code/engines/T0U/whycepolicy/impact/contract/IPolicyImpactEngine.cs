namespace Whycespace.Engines.T0U.WhycePolicy.Impact;

public interface IPolicyImpactEngine
{
    Task<ImpactAnalysisResult> AnalyzeImpactAsync(
        Guid policyId,
        CancellationToken cancellationToken = default);
}

public sealed record ImpactAnalysisResult(
    Guid PolicyId,
    int AffectedEntities,
    IReadOnlyList<string> AffectedScopes);
