using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Governance.GovernanceCycle;

public sealed class GovernanceCyclePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
