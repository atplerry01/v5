namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentSLAService
{
    public bool IsResponseBreached(IncidentBaseAggregate incident, DateTimeOffset now)
        => incident.SLA.IsResponseBreached(incident.CreatedAt, now);

    public bool IsResolutionBreached(IncidentBaseAggregate incident, DateTimeOffset now)
        => incident.SLA.IsResolutionBreached(incident.CreatedAt, now);

    public TimeSpan RemainingResponseTime(IncidentBaseAggregate incident, DateTimeOffset now)
    {
        var remaining = incident.SLA.ResponseTime - (now - incident.CreatedAt);
        return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }

    public TimeSpan RemainingResolutionTime(IncidentBaseAggregate incident, DateTimeOffset now)
    {
        var remaining = incident.SLA.ResolutionTime - (now - incident.CreatedAt);
        return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }
}
