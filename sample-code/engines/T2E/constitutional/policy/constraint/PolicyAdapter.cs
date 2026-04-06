using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Constitutional.Policy.Constraint;

public sealed class ConstraintPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
