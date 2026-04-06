using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Logistic.Tracking;

public sealed class TrackingPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
