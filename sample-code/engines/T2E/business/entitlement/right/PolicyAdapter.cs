using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Entitlement.Right;

public sealed class RightPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
