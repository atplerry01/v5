using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Portfolio.Mandate;

public sealed class MandatePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
