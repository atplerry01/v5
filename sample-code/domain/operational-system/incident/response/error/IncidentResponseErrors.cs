using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Incident.Response;

public sealed class IncidentNotHaltedException : DomainException
{
    public IncidentNotHaltedException()
        : base("INCIDENT_NOT_HALTED", "Cannot investigate until incident is halted.") { }
}
