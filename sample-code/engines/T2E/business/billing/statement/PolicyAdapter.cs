using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Billing.Statement;

public sealed class StatementPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
