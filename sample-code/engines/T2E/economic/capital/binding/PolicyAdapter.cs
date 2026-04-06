using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Capital.Binding;

public sealed class BindingPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
