namespace Whycespace.Domain.IntelligenceSystem.Economic.Analysis;

public sealed record Volume
{
    public decimal Value { get; }

    public Volume(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Volume cannot be negative.", nameof(value));
        Value = value;
    }

    public static Volume Zero => new(0);
    public Volume Add(Volume other) => new(Value + other.Value);
}
