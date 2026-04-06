using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Risk.Threshold;

public sealed class ThresholdPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
