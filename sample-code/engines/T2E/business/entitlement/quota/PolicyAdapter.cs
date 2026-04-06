using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Entitlement.Quota;

public sealed class QuotaPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
