using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Risk.Assessment;

public sealed class AssessmentPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
