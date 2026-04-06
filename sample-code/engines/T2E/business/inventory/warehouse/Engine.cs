namespace Whycespace.Engines.T2E.Business.Inventory.Warehouse;

public class WarehouseEngine
{
    private readonly WarehousePolicyAdapter _policy;

    public WarehouseEngine(WarehousePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<WarehouseResult> ExecuteAsync(WarehouseCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new WarehouseResult(true, "Executed");
    }
}
