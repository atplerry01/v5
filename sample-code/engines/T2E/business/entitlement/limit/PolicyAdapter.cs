using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Entitlement.Limit;

public sealed class LimitPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
