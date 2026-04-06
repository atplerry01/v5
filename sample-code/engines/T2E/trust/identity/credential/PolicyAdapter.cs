using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Trust.Identity.Credential;

public sealed class CredentialPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
