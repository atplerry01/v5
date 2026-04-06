using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Inventory.Count;

public sealed class CountPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
