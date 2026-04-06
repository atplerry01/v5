using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Subscription.Usage;

public sealed class UsagePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
