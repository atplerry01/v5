namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public static class ScheduleErrors
{
    public static ScheduleDomainException MissingId()
        => new("ScheduleId is required and must not be empty.");

    public static ScheduleDomainException AlreadyDeactivated(ScheduleId id)
        => new($"Schedule '{id.Value}' has already been deactivated.");

    public static ScheduleDomainException InvalidStateTransition(ScheduleStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class ScheduleDomainException : Exception
{
    public ScheduleDomainException(string message) : base(message) { }
}
