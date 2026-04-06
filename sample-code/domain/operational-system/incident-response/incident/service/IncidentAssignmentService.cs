using Whycespace.Domain.SharedKernel;
using Whycespace.Domain.SharedKernel.Primitive.Identity;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentAssignmentService
{
    public void Assign(IncidentBaseAggregate incident, IdentityId assigneeIdentityId, int escalationLevel = 1)
    {
        if (incident.Status.IsTerminal)
            throw new DomainException(IncidentErrors.NotActive, "Cannot assign a terminal incident.");

        incident.Assign(assigneeIdentityId, escalationLevel);
    }
}
