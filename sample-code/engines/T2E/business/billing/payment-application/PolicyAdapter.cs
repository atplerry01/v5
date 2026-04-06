using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Billing.PaymentApplication;

public sealed class PaymentApplicationPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
