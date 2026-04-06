namespace Whycespace.Engines.T0U.WhycePolicy.Impact;

public sealed class PolicyImpactHandler : IPolicyImpactEngine
{
    public Task<ImpactAnalysisResult> AnalyzeImpactAsync(
        Guid policyId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ImpactAnalysisResult(policyId, 0, []));
    }
}
