using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Revenue.Revenue;

public sealed class RevenuePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
