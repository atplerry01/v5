using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Localization.RegionalRule;

public sealed class RegionalRulePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
