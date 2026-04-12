namespace Whycespace.Domain.BusinessSystem.Integration.Callback;

public readonly record struct CallbackId
{
    public Guid Value { get; }

    public CallbackId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CallbackId value must not be empty.", nameof(value));
        Value = value;
    }
}
