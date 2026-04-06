using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Constitutional.Chain.Ledger;

public sealed class ChainLedgerPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
