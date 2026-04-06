using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Trust.Access.Permission;

public sealed class PermissionPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
