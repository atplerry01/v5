using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Risk.Control;

public sealed class ControlPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
