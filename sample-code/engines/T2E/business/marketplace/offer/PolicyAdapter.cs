using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Marketplace.Offer;

public sealed class OfferPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
