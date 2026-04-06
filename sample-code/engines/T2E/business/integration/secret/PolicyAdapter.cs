using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Integration.Secret;

public sealed class SecretPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
