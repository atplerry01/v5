using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Billing.Receivable;

public sealed class ReceivablePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
