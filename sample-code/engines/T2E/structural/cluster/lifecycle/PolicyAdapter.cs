using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Structural.Cluster.Lifecycle;

public sealed class LifecyclePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
