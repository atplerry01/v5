using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Marketplace.Catalog;

public sealed class CatalogPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
