namespace Whycespace.Engines.T0U.WhycePolicy.Safeguard;

public sealed class PolicySafeguardHandler : IPolicySafeguardEngine
{
    public Task<SafeguardResult> ValidateSafeguardsAsync(
        Guid policyId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SafeguardResult(true, []));
    }
}
