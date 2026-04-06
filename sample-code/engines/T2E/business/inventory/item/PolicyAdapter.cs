using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Inventory.Item;

public sealed class ItemPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
