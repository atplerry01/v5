namespace Whycespace.Engines.T2E.Business.Inventory.Adjustment;

public class AdjustmentEngine
{
    private readonly AdjustmentPolicyAdapter _policy;

    public AdjustmentEngine(AdjustmentPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AdjustmentResult> ExecuteAsync(AdjustmentCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new AdjustmentResult(true, "Executed");
    }
}
