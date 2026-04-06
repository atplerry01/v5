using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Entitlement.UsageRight;

public sealed class UsageRightPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
