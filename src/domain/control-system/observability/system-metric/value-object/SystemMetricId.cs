namespace Whycespace.Domain.ControlSystem.Observability.SystemMetric;

public readonly record struct SystemMetricId
{
    public string Value { get; }
    public SystemMetricId(string value)
    {
        if (string.IsNullOrEmpty(value)) throw SystemMetricErrors.IdMustNotBeEmpty();
        if (value.Length != 64 || !value.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
            throw SystemMetricErrors.IdMustBe64HexChars(value);
        Value = value;
    }
    public override string ToString() => Value;
}
