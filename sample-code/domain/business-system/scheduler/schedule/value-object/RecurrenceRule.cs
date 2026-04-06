namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed record RecurrenceRule(string CronExpression, string TimeZone)
{
    public static RecurrenceRule Once(string timeZone) => new("", timeZone);

    public bool IsRecurring => !string.IsNullOrWhiteSpace(CronExpression);
}
