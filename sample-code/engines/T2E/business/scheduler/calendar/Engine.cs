namespace Whycespace.Engines.T2E.Business.Scheduler.Calendar;

public class CalendarEngine
{
    private readonly CalendarPolicyAdapter _policy;

    public CalendarEngine(CalendarPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<CalendarResult> ExecuteAsync(CalendarCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new CalendarResult(true, "Executed");
    }
}
