using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Subscription.Plan;

public sealed class PlanPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
