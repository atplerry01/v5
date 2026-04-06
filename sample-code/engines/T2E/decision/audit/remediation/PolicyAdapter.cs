using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Audit.Remediation;

public sealed class RemediationPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
