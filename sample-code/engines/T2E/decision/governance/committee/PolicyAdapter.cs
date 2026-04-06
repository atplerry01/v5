using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Governance.Committee;

public sealed class CommitteePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
