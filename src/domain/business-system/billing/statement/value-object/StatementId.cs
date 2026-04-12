namespace Whycespace.Domain.BusinessSystem.Billing.Statement;

public readonly record struct StatementId
{
    public Guid Value { get; }

    public StatementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("StatementId value must not be empty.", nameof(value));

        Value = value;
    }
}
