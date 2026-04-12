namespace Whycespace.Domain.CoreSystem.Event.EventEnvelope;

public static class EventEnvelopeErrors
{
    public static InvalidOperationException MissingId() =>
        new("EventEnvelopeId is required and must not be empty.");

    public static InvalidOperationException MissingMetadata() =>
        new("Event envelope must include valid metadata.");

    public static InvalidOperationException InvalidStateTransition(EventEnvelopeStatus current, string attemptedAction) =>
        new($"Cannot '{attemptedAction}' when current status is '{current}'.");
}
