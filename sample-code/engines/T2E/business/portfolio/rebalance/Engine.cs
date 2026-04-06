namespace Whycespace.Engines.T2E.Business.Portfolio.Rebalance;

public class RebalanceEngine
{
    private readonly RebalancePolicyAdapter _policy;

    public RebalanceEngine(RebalancePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<RebalanceResult> ExecuteAsync(RebalanceCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new RebalanceResult(true, "Executed");
    }
}
