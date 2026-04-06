namespace Whycespace.Engines.T2E.Business.Portfolio.Portfolio;

public class PortfolioEngine
{
    private readonly PortfolioPolicyAdapter _policy;

    public PortfolioEngine(PortfolioPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<PortfolioResult> ExecuteAsync(PortfolioCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new PortfolioResult(true, "Executed");
    }
}
