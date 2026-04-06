using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Governance.Proposal;

public sealed class ProposalPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
