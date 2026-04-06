namespace Whycespace.Domain.IntelligenceSystem.Observability.Alert;

public readonly record struct ThresholdValue(decimal Value)
{
    public static ThresholdValue From(decimal value) => new(value);
    public override string ToString() => Value.ToString();
    public static implicit operator decimal(ThresholdValue t) => t.Value;
    public static implicit operator ThresholdValue(decimal value) => new(value);
}
