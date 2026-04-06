using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Inventory.Transfer;

public sealed class TransferPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
