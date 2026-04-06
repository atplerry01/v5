using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Resource.Utilization;

public sealed class UtilizationPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
