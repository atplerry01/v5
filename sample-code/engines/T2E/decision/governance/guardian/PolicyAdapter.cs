using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Governance.Guardian;

public sealed class GuardianPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
