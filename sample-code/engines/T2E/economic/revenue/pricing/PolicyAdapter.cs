using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Revenue.Pricing;

public sealed class PricingPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
