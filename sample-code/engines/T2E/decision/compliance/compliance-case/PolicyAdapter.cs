using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Compliance.ComplianceCase;

public sealed class ComplianceCasePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
