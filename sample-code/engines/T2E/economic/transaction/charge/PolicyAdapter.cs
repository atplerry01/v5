using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Transaction.Charge;

public sealed class ChargePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
