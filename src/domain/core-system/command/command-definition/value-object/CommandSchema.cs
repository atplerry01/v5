namespace Whycespace.Domain.CoreSystem.Command.CommandDefinition;

public readonly record struct CommandSchema
{
    public string CommandName { get; }
    public int SchemaVersion { get; }

    public CommandSchema(string commandName, int schemaVersion)
    {
        if (string.IsNullOrWhiteSpace(commandName))
            throw new ArgumentException("Command name must not be empty.", nameof(commandName));

        if (schemaVersion <= 0)
            throw new ArgumentException("Schema version must be positive.", nameof(schemaVersion));

        CommandName = commandName;
        SchemaVersion = schemaVersion;
    }
}
