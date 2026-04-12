namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public readonly record struct ChargeId
{
    public Guid Value { get; }

    public ChargeId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ChargeId cannot be empty.", nameof(value));
        Value = value;
    }

    public static ChargeId From(Guid value) => new(value);
}
