namespace Whycespace.Domain.BusinessSystem.Integration.CommandBridge;

public readonly record struct CommandMappingId
{
    public Guid Value { get; }

    public CommandMappingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommandMappingId value must not be empty.", nameof(value));
        Value = value;
    }
}
