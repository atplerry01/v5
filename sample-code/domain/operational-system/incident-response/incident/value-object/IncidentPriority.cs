namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed record IncidentPriority(string Value)
{
    public static readonly IncidentPriority P1 = new("P1");
    public static readonly IncidentPriority P2 = new("P2");
    public static readonly IncidentPriority P3 = new("P3");
    public static readonly IncidentPriority P4 = new("P4");

    public static IncidentPriority FromSeverity(IncidentSeverity severity) => severity.Value switch
    {
        "critical" => P1,
        "high" => P2,
        "medium" => P3,
        "low" => P4,
        _ => P3
    };

    public override string ToString() => Value;
}
