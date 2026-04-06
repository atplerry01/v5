using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Scheduler.Availability;

public sealed class AvailabilityPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
