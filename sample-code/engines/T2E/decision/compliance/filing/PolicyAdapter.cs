using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Compliance.Filing;

public sealed class FilingPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
