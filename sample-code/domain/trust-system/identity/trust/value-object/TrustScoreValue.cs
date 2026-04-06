namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public sealed record TrustScoreValue
{
    public decimal Value { get; }

    public TrustScoreValue(decimal value)
    {
        Value = Math.Clamp(value, 0m, 100m);
    }

    public static TrustScoreValue Initial => new(0m);

    public static TrustScoreValue Compute(TrustScoreValue current, decimal weight)
    {
        return new TrustScoreValue(current.Value + weight);
    }

    public override string ToString() => Value.ToString("F2");
}
