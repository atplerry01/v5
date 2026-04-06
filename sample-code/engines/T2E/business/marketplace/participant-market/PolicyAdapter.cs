using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Marketplace.ParticipantMarket;

public sealed class ParticipantMarketPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
