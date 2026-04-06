using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Audit.AuditCase;

public sealed class AuditCasePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
