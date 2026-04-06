namespace Whycespace.Engines.T2E.Business.Inventory.Sku;

public class SkuEngine
{
    private readonly SkuPolicyAdapter _policy;

    public SkuEngine(SkuPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<SkuResult> ExecuteAsync(SkuCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new SkuResult(true, "Executed");
    }
}
