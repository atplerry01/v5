using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Inventory.Warehouse;

public sealed class WarehousePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
