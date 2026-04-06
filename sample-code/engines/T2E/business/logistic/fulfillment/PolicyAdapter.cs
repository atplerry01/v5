using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Logistic.Fulfillment;

public sealed class FulfillmentPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
