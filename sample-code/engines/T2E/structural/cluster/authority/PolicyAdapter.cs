using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Structural.Cluster.Authority;

public sealed class AuthorityPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
