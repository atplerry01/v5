namespace Whycespace.Engines.T2E.Business.Scheduler.Schedule;

public class ScheduleEngine
{
    private readonly SchedulePolicyAdapter _policy;

    public ScheduleEngine(SchedulePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ScheduleResult> ExecuteAsync(ScheduleCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ScheduleResult(true, "Executed");
    }
}
