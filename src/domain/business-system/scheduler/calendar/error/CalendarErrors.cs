namespace Whycespace.Domain.BusinessSystem.Scheduler.Calendar;

public static class CalendarErrors
{
    public static CalendarDomainException MissingId()
        => new("CalendarId is required and must not be empty.");

    public static CalendarDomainException AlreadyArchived(CalendarId id)
        => new($"Calendar '{id.Value}' has already been archived.");

    public static CalendarDomainException InvalidStateTransition(CalendarStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class CalendarDomainException : Exception
{
    public CalendarDomainException(string message) : base(message) { }
}
