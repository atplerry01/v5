using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Integration.Registry;

public sealed class RegistryPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
