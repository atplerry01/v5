using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Audit.Access;

public sealed class AccessPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
