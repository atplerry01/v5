namespace Whycespace.Domain.CoreSystem.Event.EventStream;

public static class EventStreamErrors
{
    public static EventStreamDomainException MissingId()
        => new("EventStreamId is required and must not be empty.");

    public static EventStreamDomainException MissingDescriptor()
        => new("StreamDescriptor is required and must not be default.");

    public static EventStreamDomainException InvalidStateTransition(EventStreamStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class EventStreamDomainException : InvalidOperationException
{
    public EventStreamDomainException(string message) : base(message) { }
}
