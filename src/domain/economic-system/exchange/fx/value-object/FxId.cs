namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

public readonly record struct FxId
{
    public Guid Value { get; }

    public FxId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FxId cannot be empty.", nameof(value));

        Value = value;
    }

    public static FxId From(Guid value) => new(value);
}
