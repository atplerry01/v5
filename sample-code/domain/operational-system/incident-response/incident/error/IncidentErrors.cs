namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public static class IncidentErrors
{
    public const string AlreadyClosed = "INCIDENT_ALREADY_CLOSED";
    public const string AlreadyResolved = "INCIDENT_ALREADY_RESOLVED";
    public const string InvalidTransition = "INCIDENT_INVALID_TRANSITION";
    public const string InvalidDescription = "INCIDENT_INVALID_DESCRIPTION";
    public const string CannotEscalate = "INCIDENT_CANNOT_ESCALATE";
    public const string NotActive = "INCIDENT_NOT_ACTIVE";
    public const string NotAssigned = "INCIDENT_NOT_ASSIGNED";
    public const string AlreadyAssigned = "INCIDENT_ALREADY_ASSIGNED";
    public const string SLABreached = "INCIDENT_SLA_BREACHED";
}
