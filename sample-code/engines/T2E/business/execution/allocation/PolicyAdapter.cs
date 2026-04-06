using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Execution.Allocation;

public sealed class AllocationPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
