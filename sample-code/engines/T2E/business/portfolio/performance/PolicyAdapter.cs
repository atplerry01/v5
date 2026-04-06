using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Portfolio.Performance;

public sealed class PerformancePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
