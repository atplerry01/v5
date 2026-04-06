using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Agreement.Approval;

public sealed class ApprovalPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
