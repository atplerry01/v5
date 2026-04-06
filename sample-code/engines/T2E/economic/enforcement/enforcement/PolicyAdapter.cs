using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Enforcement;

public sealed class EnforcementPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
