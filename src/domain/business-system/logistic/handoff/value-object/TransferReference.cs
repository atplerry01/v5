namespace Whycespace.Domain.BusinessSystem.Logistic.Handoff;

public readonly record struct TransferReference
{
    public Guid Value { get; }

    public TransferReference(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TransferReference value must not be empty.", nameof(value));
        Value = value;
    }
}
