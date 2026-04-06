using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Billing.Adjustment;

public sealed class AdjustmentPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
