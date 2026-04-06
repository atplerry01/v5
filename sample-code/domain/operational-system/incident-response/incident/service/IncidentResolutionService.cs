using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentResolutionService
{
    public void Resolve(IncidentBaseAggregate incident)
    {
        var spec = new IncidentResolutionSpec();
        if (!spec.IsSatisfiedBy(incident))
            throw new DomainException(IncidentErrors.NotAssigned, "Cannot resolve an unassigned incident.");

        incident.Resolve();
    }
}
