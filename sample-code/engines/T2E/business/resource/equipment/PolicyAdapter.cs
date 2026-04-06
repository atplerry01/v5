using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Resource.Equipment;

public sealed class EquipmentPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
