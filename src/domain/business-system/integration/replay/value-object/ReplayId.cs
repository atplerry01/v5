namespace Whycespace.Domain.BusinessSystem.Integration.Replay;

public readonly record struct ReplayId
{
    public Guid Value { get; }

    public ReplayId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ReplayId value must not be empty.", nameof(value));
        Value = value;
    }
}
