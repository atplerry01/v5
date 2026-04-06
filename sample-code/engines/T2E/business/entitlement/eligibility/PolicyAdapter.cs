using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Entitlement.Eligibility;

public sealed class EligibilityPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
