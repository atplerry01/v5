using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Audit.Finding;

public sealed class FindingPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
