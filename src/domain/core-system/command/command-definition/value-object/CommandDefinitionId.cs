namespace Whycespace.Domain.CoreSystem.Command.CommandDefinition;

public readonly record struct CommandDefinitionId
{
    public Guid Value { get; }

    public CommandDefinitionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommandDefinitionId cannot be empty.", nameof(value));

        Value = value;
    }
}
