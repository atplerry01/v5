namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed record IncidentSLA(
    TimeSpan ResponseTime,
    TimeSpan ResolutionTime)
{
    public static IncidentSLA ForSeverity(IncidentSeverity severity) => severity.Value switch
    {
        "critical" => new IncidentSLA(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(30)),
        "high" => new IncidentSLA(TimeSpan.FromMinutes(15), TimeSpan.FromHours(2)),
        "medium" => new IncidentSLA(TimeSpan.FromHours(1), TimeSpan.FromHours(6)),
        "low" => new IncidentSLA(TimeSpan.FromHours(4), TimeSpan.FromHours(24)),
        _ => new IncidentSLA(TimeSpan.FromHours(1), TimeSpan.FromHours(6))
    };

    public bool IsResponseBreached(DateTimeOffset createdAt, DateTimeOffset now)
        => (now - createdAt) > ResponseTime;

    public bool IsResolutionBreached(DateTimeOffset createdAt, DateTimeOffset now)
        => (now - createdAt) > ResolutionTime;
}
