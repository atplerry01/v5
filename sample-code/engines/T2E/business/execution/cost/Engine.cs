namespace Whycespace.Engines.T2E.Business.Execution.Cost;

public class CostEngine
{
    private readonly CostPolicyAdapter _policy;

    public CostEngine(CostPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<CostResult> ExecuteAsync(CostCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new CostResult(true, "Executed");
    }
}
