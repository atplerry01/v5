namespace Whycespace.Domain.BusinessSystem.Scheduler.Recurrence;

public sealed record TriggerType(string Value)
{
    public static readonly TriggerType Cron = new("cron");
    public static readonly TriggerType OneTime = new("one_time");
    public static readonly TriggerType Interval = new("interval");
    public static readonly TriggerType EventDriven = new("event_driven");
}
