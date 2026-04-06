using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Portfolio.Benchmark;

public sealed class BenchmarkPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
