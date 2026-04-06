namespace Whycespace.Engines.T2E.Business.Inventory.Stock;

public class StockEngine
{
    private readonly StockPolicyAdapter _policy;

    public StockEngine(StockPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<StockResult> ExecuteAsync(StockCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new StockResult(true, "Executed");
    }
}
