using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Notification.Delivery;

public sealed class DeliveryPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
