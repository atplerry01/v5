namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public readonly record struct SliceId
{
    public Guid Value { get; }

    public SliceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SliceId cannot be empty.", nameof(value));

        Value = value;
    }

    public static SliceId From(Guid value) => new(value);
}
