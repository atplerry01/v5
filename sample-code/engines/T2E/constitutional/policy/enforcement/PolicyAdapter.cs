using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Constitutional.Policy.Enforcement;

public sealed class PolicyEnforcementPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
