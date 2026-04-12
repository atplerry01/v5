namespace Whycespace.Domain.BusinessSystem.Execution.Activation;

public readonly record struct ActivationTargetId
{
    public Guid Value { get; }

    public ActivationTargetId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ActivationTargetId value must not be empty.", nameof(value));
        Value = value;
    }
}
