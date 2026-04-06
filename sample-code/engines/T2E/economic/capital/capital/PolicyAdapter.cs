using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Capital.Capital;

public sealed class CapitalPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
