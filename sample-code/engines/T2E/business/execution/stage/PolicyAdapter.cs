using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Execution.Stage;

public sealed class StagePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
