using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Agreement.Clause;

public sealed class ClausePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
