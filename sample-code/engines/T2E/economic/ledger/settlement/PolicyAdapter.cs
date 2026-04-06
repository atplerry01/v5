using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Ledger.Settlement;

public sealed class SettlementPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
