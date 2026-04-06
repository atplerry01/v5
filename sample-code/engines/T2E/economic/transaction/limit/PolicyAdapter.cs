using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Transaction.Limit;

public sealed class LimitPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
