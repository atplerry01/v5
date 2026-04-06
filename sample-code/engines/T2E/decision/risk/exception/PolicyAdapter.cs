using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Risk.Exception;

public sealed class ExceptionPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
