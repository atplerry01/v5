namespace Whycespace.Domain.BusinessSystem.Integration.CommandBridge;

public readonly record struct CommandBridgeId
{
    public Guid Value { get; }

    public CommandBridgeId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommandBridgeId value must not be empty.", nameof(value));
        Value = value;
    }
}
