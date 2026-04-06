using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Risk.Alert;

public sealed class AlertPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
