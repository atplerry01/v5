namespace Whycespace.Domain.BusinessSystem.Integration.Synchronization;

public readonly record struct SyncPolicyId
{
    public Guid Value { get; }

    public SyncPolicyId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SyncPolicyId value must not be empty.", nameof(value));

        Value = value;
    }
}
