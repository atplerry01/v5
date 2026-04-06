using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Resource.AssetResource;

public sealed class AssetResourcePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
