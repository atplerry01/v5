using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Subscription.Enrollment;

public sealed class EnrollmentPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
