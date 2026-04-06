namespace Whycespace.Engines.T2E.Business.Notification.Delivery;

public class DeliveryEngine
{
    private readonly DeliveryPolicyAdapter _policy;

    public DeliveryEngine(DeliveryPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<DeliveryResult> ExecuteAsync(DeliveryCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new DeliveryResult(true, "Executed");
    }
}
