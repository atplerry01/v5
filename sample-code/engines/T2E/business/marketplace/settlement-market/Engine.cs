namespace Whycespace.Engines.T2E.Business.Marketplace.SettlementMarket;

public class SettlementMarketEngine
{
    private readonly SettlementMarketPolicyAdapter _policy;

    public SettlementMarketEngine(SettlementMarketPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<SettlementMarketResult> ExecuteAsync(SettlementMarketCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new SettlementMarketResult(true, "Executed");
    }
}
