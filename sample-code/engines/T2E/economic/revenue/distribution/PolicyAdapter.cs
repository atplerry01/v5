using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Revenue.Distribution;

public sealed class DistributionPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
