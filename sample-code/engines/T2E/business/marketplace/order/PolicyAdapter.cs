using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Marketplace.Order;

public sealed class OrderPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
