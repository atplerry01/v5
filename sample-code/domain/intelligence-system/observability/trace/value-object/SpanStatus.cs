namespace Whycespace.Domain.IntelligenceSystem.Observability.Trace;

/// <summary>
/// Represents the outcome status of a trace span.
/// </summary>
public sealed record SpanStatus
{
    public string Value { get; }

    private SpanStatus(string value) => Value = value;

    public static readonly SpanStatus Unset = new("Unset");
    public static readonly SpanStatus Ok = new("Ok");
    public static readonly SpanStatus Error = new("Error");

    public override string ToString() => Value;
}
