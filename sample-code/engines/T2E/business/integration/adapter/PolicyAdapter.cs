using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Integration.Adapter;

public sealed class AdapterPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
