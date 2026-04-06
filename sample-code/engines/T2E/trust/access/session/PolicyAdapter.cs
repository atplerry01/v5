using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Trust.Access.Session;

public sealed class SessionPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
