namespace Whycespace.Domain.BusinessSystem.Scheduler.Recurrence;

public static class RecurrenceErrors
{
    public static RecurrenceDomainException MissingId()
        => new("RecurrenceId is required and must not be empty.");

    public static RecurrenceDomainException InvalidPattern()
        => new("Recurrence pattern must define valid frequency, interval, and bounds.");

    public static RecurrenceDomainException InvalidStateTransition(RecurrenceStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static RecurrenceDomainException AlreadyTerminated(RecurrenceId id)
        => new($"Recurrence '{id.Value}' has already been terminated.");
}

public sealed class RecurrenceDomainException : Exception
{
    public RecurrenceDomainException(string message) : base(message) { }
}
