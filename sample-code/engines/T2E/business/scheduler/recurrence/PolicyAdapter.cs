using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Scheduler.Recurrence;

public sealed class RecurrencePolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
