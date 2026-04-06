using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Ledger.Treasury;

public sealed class TreasuryPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
