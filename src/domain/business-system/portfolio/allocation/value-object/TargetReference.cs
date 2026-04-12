namespace Whycespace.Domain.BusinessSystem.Portfolio.Allocation;

public readonly record struct TargetReference
{
    public Guid Value { get; }

    public TargetReference(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TargetReference value must not be empty.", nameof(value));

        Value = value;
    }
}
