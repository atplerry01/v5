namespace Whycespace.Domain.CoreSystem.Command.CommandDefinition;

public static class CommandDefinitionErrors
{
    public static InvalidOperationException MissingId() =>
        new("CommandDefinitionId is required and must not be empty.");

    public static InvalidOperationException MissingSchema() =>
        new("Command definition must include a valid schema.");

    public static InvalidOperationException InvalidStateTransition(CommandDefinitionStatus current, string attemptedAction) =>
        new($"Cannot '{attemptedAction}' when current status is '{current}'.");
}
