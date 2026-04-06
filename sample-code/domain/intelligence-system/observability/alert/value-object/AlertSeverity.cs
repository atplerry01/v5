namespace Whycespace.Domain.IntelligenceSystem.Observability.Alert;

public sealed record AlertSeverity
{
    public string Value { get; }

    private AlertSeverity(string value) => Value = value;

    public static readonly AlertSeverity Low = new("Low");
    public static readonly AlertSeverity Medium = new("Medium");
    public static readonly AlertSeverity High = new("High");
    public static readonly AlertSeverity Critical = new("Critical");

    public static AlertSeverity From(string value) => value switch
    {
        "Low" => Low,
        "Medium" => Medium,
        "High" => High,
        "Critical" => Critical,
        _ => throw new ArgumentException($"Invalid alert severity: {value}", nameof(value))
    };

    public override string ToString() => Value;
}
