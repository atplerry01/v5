namespace Whycespace.Domain.CoreSystem.Event.EventDefinition;

public static class EventDefinitionErrors
{
    public static InvalidOperationException MissingId() =>
        new("EventDefinitionId is required and must not be empty.");

    public static InvalidOperationException MissingSchema() =>
        new("Event definition must include a valid schema.");

    public static InvalidOperationException InvalidStateTransition(EventDefinitionStatus current, string attemptedAction) =>
        new($"Cannot '{attemptedAction}' when current status is '{current}'.");
}
