namespace Whycespace.Domain.OperationalSystem.Incident.Response;

public sealed class IncidentResponseService
{
    public bool CanInvestigate(IncidentResponseAggregate incident) => incident.Status == IncidentResponseStatus.Halted;
    public bool CanResolve(IncidentResponseAggregate incident) => incident.Status == IncidentResponseStatus.Investigating;
    public bool CanClose(IncidentResponseAggregate incident) => incident.Status == IncidentResponseStatus.Resolved;
}
