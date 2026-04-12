namespace Whycespace.Domain.EconomicSystem.Capital.Reserve;

public readonly record struct ReserveId
{
    public Guid Value { get; }

    public ReserveId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ReserveId cannot be empty.", nameof(value));

        Value = value;
    }

    public static ReserveId From(Guid value) => new(value);
}
