using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Logistic.Route;

public sealed class RoutePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
