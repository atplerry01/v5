using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Marketplace.Match;

public sealed class MatchPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
