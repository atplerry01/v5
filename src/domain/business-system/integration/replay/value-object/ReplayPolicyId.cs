namespace Whycespace.Domain.BusinessSystem.Integration.Replay;

public readonly record struct ReplayPolicyId
{
    public Guid Value { get; }

    public ReplayPolicyId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ReplayPolicyId value must not be empty.", nameof(value));
        Value = value;
    }
}
