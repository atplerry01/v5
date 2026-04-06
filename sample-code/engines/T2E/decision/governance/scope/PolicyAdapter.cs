using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Governance.Scope;

public sealed class ScopePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
