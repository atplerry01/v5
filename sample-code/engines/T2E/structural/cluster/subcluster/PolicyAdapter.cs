using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Structural.Cluster.Subcluster;

public sealed class SubclusterPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
