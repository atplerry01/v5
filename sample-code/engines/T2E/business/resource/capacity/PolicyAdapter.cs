using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Resource.Capacity;

public sealed class CapacityPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
