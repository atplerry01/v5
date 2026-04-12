namespace Whycespace.Domain.BusinessSystem.Execution.Setup;

public readonly record struct SetupTargetId
{
    public Guid Value { get; }

    public SetupTargetId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SetupTargetId value must not be empty.", nameof(value));
        Value = value;
    }
}
