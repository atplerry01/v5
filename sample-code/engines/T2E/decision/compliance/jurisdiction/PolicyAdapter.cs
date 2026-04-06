using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Decision.Compliance.Jurisdiction;

public sealed class JurisdictionPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
