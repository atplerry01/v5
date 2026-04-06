using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Structural.Cluster.Classification;

public sealed class ClassificationPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
