namespace Whycespace.Engines.T2E.Business.Inventory.Item;

public class ItemEngine
{
    private readonly ItemPolicyAdapter _policy;

    public ItemEngine(ItemPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ItemResult> ExecuteAsync(ItemCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ItemResult(true, "Executed");
    }
}
