namespace Whycespace.Engines.T2E.Business.Inventory.Valuation;

public class ValuationEngine
{
    private readonly ValuationPolicyAdapter _policy;

    public ValuationEngine(ValuationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ValuationResult> ExecuteAsync(ValuationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ValuationResult(true, "Executed");
    }
}
