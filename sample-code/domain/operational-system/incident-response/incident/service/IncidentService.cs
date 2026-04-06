namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentService
{
    public IncidentBaseAggregate CreateIncident(
        Guid incidentId,
        IncidentType type,
        IncidentSeverity severity,
        IncidentSource source,
        Guid affectedEntityId,
        string description,
        IncidentReference? reference = null,
        IncidentCorrelationId sourceCorrelationId = default)
    {
        Guard.AgainstDefault(incidentId);
        Guard.AgainstNull(type);
        Guard.AgainstNull(severity);
        Guard.AgainstNull(source);
        Guard.AgainstDefault(affectedEntityId);
        Guard.AgainstEmpty(description);

        return IncidentBaseAggregate.Create(
            incidentId, type, severity, source,
            affectedEntityId, description,
            reference ?? IncidentReference.None,
            sourceCorrelationId);
    }
}
