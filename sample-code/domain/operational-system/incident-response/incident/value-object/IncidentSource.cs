namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed record IncidentSource(string Value)
{
    public static readonly IncidentSource Manual = new("manual");
    public static readonly IncidentSource System = new("system");
    public static readonly IncidentSource Alert = new("alert");
    public static readonly IncidentSource HealthCheck = new("health_check");
    public static readonly IncidentSource Policy = new("policy");

    public override string ToString() => Value;
}
