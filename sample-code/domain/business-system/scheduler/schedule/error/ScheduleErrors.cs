namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public static class ScheduleErrors
{
    public const string InvalidTimeRange = "SCHEDULE_INVALID_TIME_RANGE";
    public const string InvalidRecurrenceRule = "SCHEDULE_INVALID_RECURRENCE_RULE";
    public const string AlreadyCancelled = "SCHEDULE_ALREADY_CANCELLED";
    public const string AlreadyExpired = "SCHEDULE_ALREADY_EXPIRED";
    public const string InvalidTransition = "SCHEDULE_INVALID_TRANSITION";
}
