using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Trust.Access.Grant;

public sealed class GrantPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
