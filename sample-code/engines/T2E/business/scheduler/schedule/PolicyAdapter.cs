using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Scheduler.Schedule;

public sealed class SchedulePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
