using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Execution.Activation;

public sealed class ActivationPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
