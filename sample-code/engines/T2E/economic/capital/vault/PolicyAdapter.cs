using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Capital.Vault;

public sealed class VaultPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
