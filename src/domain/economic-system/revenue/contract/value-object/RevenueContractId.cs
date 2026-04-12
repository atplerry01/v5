namespace Whycespace.Domain.EconomicSystem.Revenue.Contract;

public readonly record struct RevenueContractId
{
    public Guid Value { get; }

    public RevenueContractId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RevenueContractId cannot be empty.", nameof(value));
        Value = value;
    }

    public static RevenueContractId From(Guid value) => new(value);
}
