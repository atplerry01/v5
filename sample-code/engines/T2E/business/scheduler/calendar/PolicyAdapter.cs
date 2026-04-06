using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Scheduler.Calendar;

public sealed class CalendarPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
