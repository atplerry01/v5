using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Ledger.Ledger;

public sealed class LedgerPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
