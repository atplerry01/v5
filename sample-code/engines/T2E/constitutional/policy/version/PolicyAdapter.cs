using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Constitutional.Policy.Version;

public sealed class PolicyVersionPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
