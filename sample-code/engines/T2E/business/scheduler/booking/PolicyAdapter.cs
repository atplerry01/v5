using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Scheduler.Booking;

public sealed class BookingPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
