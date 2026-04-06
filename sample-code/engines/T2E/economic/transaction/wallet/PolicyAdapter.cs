using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Transaction.Wallet;

public sealed class WalletPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
