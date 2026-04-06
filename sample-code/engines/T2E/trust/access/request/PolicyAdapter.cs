using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Trust.Access.Request;

public sealed class AccessRequestPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
