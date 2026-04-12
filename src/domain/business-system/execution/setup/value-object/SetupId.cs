namespace Whycespace.Domain.BusinessSystem.Execution.Setup;

public readonly record struct SetupId
{
    public Guid Value { get; }

    public SetupId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SetupId value must not be empty.", nameof(value));
        Value = value;
    }
}
