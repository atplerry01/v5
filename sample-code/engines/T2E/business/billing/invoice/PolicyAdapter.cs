using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Billing.Invoice;

public sealed class InvoicePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
