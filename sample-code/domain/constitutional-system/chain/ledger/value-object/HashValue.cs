namespace Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;

public sealed class HashValue : ValueObject
{
    public string Value { get; }

    private HashValue(string value)
    {
        Guard.AgainstEmpty(value, nameof(value));
        Guard.AgainstInvalid(value, v => v.Length == 64, "HashValue must be a 64-character hex string.");
        Value = value;
    }

    public static HashValue From(string hash) => new(hash);

    public static readonly HashValue Genesis = new("0000000000000000000000000000000000000000000000000000000000000000");

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(HashValue hash) => hash.Value;
}
