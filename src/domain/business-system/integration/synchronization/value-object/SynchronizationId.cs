namespace Whycespace.Domain.BusinessSystem.Integration.Synchronization;

public readonly record struct SynchronizationId
{
    public Guid Value { get; }

    public SynchronizationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SynchronizationId value must not be empty.", nameof(value));

        Value = value;
    }
}
