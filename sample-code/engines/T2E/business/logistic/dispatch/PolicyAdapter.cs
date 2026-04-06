using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Logistic.Dispatch;

public sealed class DispatchPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
