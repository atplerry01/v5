using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Ledger.Obligation;

public sealed class ObligationPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
