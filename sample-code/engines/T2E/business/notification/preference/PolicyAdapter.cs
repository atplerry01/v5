using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Notification.Preference;

public sealed class PreferencePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
