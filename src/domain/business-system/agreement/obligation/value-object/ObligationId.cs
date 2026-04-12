namespace Whycespace.Domain.BusinessSystem.Agreement.Obligation;

public readonly record struct ObligationId
{
    public Guid Value { get; }

    public ObligationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ObligationId value must not be empty.", nameof(value));

        Value = value;
    }
}
