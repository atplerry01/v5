using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Compliance.Regulation;

public sealed class RegulationPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
