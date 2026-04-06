using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Inventory.Valuation;

public sealed class ValuationPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
