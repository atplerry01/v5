using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Trust.Access.Role;

public sealed class RolePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
