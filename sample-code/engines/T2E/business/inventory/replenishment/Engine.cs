namespace Whycespace.Engines.T2E.Business.Inventory.Replenishment;

public class ReplenishmentEngine
{
    private readonly ReplenishmentPolicyAdapter _policy;

    public ReplenishmentEngine(ReplenishmentPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ReplenishmentResult> ExecuteAsync(ReplenishmentCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ReplenishmentResult(true, "Executed");
    }
}
