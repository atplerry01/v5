using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Subscription.SubscriptionAccount;

public sealed class SubscriptionAccountPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
