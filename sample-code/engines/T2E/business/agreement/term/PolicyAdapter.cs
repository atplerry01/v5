using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Agreement.Term;

public sealed class TermPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
