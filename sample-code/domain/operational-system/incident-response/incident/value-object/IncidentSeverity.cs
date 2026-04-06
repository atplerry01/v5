namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed record IncidentSeverity(string Value)
{
    public static readonly IncidentSeverity Low = new("low");
    public static readonly IncidentSeverity Medium = new("medium");
    public static readonly IncidentSeverity High = new("high");
    public static readonly IncidentSeverity Critical = new("critical");

    public bool CanEscalate => this != Critical;

    public IncidentSeverity Escalate() => Value switch
    {
        "low" => Medium,
        "medium" => High,
        "high" => Critical,
        _ => throw new InvalidOperationException("Cannot escalate beyond critical severity.")
    };
}
