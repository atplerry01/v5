using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Subscription.Renewal;

public sealed class RenewalPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
