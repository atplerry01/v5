namespace Whycespace.Domain.BusinessSystem.Logistic.Dispatch;

public readonly record struct DispatchId
{
    public Guid Value { get; }

    public DispatchId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DispatchId value must not be empty.", nameof(value));

        Value = value;
    }
}
