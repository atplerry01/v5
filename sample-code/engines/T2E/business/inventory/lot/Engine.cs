namespace Whycespace.Engines.T2E.Business.Inventory.Lot;

public class LotEngine
{
    private readonly LotPolicyAdapter _policy;

    public LotEngine(LotPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<LotResult> ExecuteAsync(LotCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new LotResult(true, "Executed");
    }
}
