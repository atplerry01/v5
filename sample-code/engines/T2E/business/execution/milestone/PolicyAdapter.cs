using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Execution.Milestone;

public sealed class MilestonePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
