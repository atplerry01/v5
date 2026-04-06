namespace Whycespace.Engines.T2E.Business.Marketplace.Catalog;

public class CatalogEngine
{
    private readonly CatalogPolicyAdapter _policy;

    public CatalogEngine(CatalogPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<CatalogResult> ExecuteAsync(CatalogCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new CatalogResult(true, "Executed");
    }
}
