namespace Whycespace.Domain.BusinessSystem.Execution.Activation;

public readonly record struct ActivationId
{
    public Guid Value { get; }

    public ActivationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ActivationId value must not be empty.", nameof(value));
        Value = value;
    }
}
