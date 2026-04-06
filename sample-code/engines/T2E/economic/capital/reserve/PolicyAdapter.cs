using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Capital.Reserve;

public sealed class ReservePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
