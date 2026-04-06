namespace Whycespace.Engines.T2E.Business.Logistic.Shipment;

public class ShipmentEngine
{
    private readonly ShipmentPolicyAdapter _policy;

    public ShipmentEngine(ShipmentPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ShipmentResult> ExecuteAsync(ShipmentCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ShipmentResult(true, "Executed");
    }
}
