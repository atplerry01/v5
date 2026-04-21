using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandDefinition;

public readonly record struct CommandSchema
{
    public string CommandName { get; }
    public int SchemaVersion { get; }

    public CommandSchema(string commandName, int schemaVersion)
    {
        Guard.Against(string.IsNullOrWhiteSpace(commandName), "Command name must not be empty.");
        Guard.Against(schemaVersion <= 0, "Schema version must be positive.");

        CommandName = commandName;
        SchemaVersion = schemaVersion;
    }
}
