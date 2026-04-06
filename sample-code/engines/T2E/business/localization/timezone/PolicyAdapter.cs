using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Localization.Timezone;

public sealed class TimezonePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
