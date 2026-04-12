namespace Whycespace.Domain.CoreSystem.Command.CommandDefinition;

public sealed record CommandDefinitionRegisteredEvent(
    CommandDefinitionId DefinitionId,
    CommandSchema Schema);
