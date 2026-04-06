using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Trust.Identity.IdentityGraph;

public sealed class IdentityGraphPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
