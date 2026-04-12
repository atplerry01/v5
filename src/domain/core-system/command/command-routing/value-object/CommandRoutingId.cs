namespace Whycespace.Domain.CoreSystem.Command.CommandRouting;

public readonly record struct CommandRoutingId
{
    public Guid Value { get; }

    public CommandRoutingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommandRoutingId cannot be empty.", nameof(value));

        Value = value;
    }
}
