namespace Whycespace.Domain.BusinessSystem.Agreement.Amendment;

public readonly record struct AmendmentId
{
    public Guid Value { get; }

    public AmendmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AmendmentId value must not be empty.", nameof(value));

        Value = value;
    }
}
