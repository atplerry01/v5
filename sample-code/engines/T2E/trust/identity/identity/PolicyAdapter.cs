using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Trust.Identity.Identity;

public sealed class IdentityPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
