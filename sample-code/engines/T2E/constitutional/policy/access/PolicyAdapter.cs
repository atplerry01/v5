using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Constitutional.Policy.Access;

public sealed class PolicyAccessPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
