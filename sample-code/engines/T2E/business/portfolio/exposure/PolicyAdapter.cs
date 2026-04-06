using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Portfolio.Exposure;

public sealed class ExposurePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
