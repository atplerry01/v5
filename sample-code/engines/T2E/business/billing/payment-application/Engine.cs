namespace Whycespace.Engines.T2E.Business.Billing.PaymentApplication;

public class PaymentApplicationEngine
{
    private readonly PaymentApplicationPolicyAdapter _policy;

    public PaymentApplicationEngine(PaymentApplicationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<PaymentApplicationResult> ExecuteAsync(PaymentApplicationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new PaymentApplicationResult(true, "Executed");
    }
}
