using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Audit.EvidenceAudit;

public sealed class EvidenceAuditPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
