using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Capital.Asset;

public sealed class AssetPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
