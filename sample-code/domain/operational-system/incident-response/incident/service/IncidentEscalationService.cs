using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentEscalationService
{
    public void EscalateIfBreached(IncidentBaseAggregate incident, DateTimeOffset now)
    {
        if (!incident.Status.IsActive)
            return;

        if (incident.SLA.IsResponseBreached(incident.CreatedAt, now)
            && incident.CurrentAssignment is null)
        {
            incident.Escalate();
        }

        if (incident.SLA.IsResolutionBreached(incident.CreatedAt, now))
        {
            incident.Escalate();
        }
    }
}
