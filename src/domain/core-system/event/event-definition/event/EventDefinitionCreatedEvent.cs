namespace Whycespace.Domain.CoreSystem.Event.EventDefinition;

public sealed record EventDefinitionRegisteredEvent(
    EventDefinitionId DefinitionId,
    EventSchema Schema);
