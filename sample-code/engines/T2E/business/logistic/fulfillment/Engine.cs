namespace Whycespace.Engines.T2E.Business.Logistic.Fulfillment;

public class FulfillmentEngine
{
    private readonly FulfillmentPolicyAdapter _policy;

    public FulfillmentEngine(FulfillmentPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<FulfillmentResult> ExecuteAsync(FulfillmentCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new FulfillmentResult(true, "Executed");
    }
}
