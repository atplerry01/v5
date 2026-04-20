namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Acceptance;

public readonly record struct AcceptanceId
{
    public Guid Value { get; }

    public AcceptanceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AcceptanceId value must not be empty.", nameof(value));

        Value = value;
    }
}
