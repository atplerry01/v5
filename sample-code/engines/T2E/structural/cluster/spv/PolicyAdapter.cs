using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Structural.Cluster.Spv;

public sealed class SpvPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
