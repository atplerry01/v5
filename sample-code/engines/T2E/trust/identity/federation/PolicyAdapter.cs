using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Trust.Identity.Federation;

public sealed class FederationPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
