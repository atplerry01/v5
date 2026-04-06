using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Resource.MaintenanceResource;

public sealed class MaintenanceResourcePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
