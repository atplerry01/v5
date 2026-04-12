namespace Whycespace.Domain.BusinessSystem.Integration.EventBridge;

public static class EventBridgeErrors
{
    public static EventBridgeDomainException MissingId()
        => new("EventBridgeId is required and must not be empty.");

    public static EventBridgeDomainException MissingMappingId()
        => new("EventMappingId is required and must not be empty.");

    public static EventBridgeDomainException InvalidStateTransition(EventBridgeStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static EventBridgeDomainException AlreadyActive(EventBridgeId id)
        => new($"EventBridge '{id.Value}' is already active.");

    public static EventBridgeDomainException AlreadyDisabled(EventBridgeId id)
        => new($"EventBridge '{id.Value}' is already disabled.");
}

public sealed class EventBridgeDomainException : Exception
{
    public EventBridgeDomainException(string message) : base(message) { }
}
