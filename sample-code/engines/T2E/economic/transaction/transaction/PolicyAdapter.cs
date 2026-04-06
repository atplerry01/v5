using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Transaction.Transaction;

public sealed class TransactionPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
