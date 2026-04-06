using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Notification.Subscription;

public sealed class SubscriptionPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
