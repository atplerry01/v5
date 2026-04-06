using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Revenue.Payout;

public sealed class PayoutPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
