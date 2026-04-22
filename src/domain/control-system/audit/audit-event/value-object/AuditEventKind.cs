namespace Whycespace.Domain.ControlSystem.Audit.AuditEvent;

public enum AuditEventKind
{
    AccessDecision = 1,
    PolicyEvaluation = 2,
    ConfigurationChange = 3,
    IdentityAction = 4,
    SystemAction = 5,
    SecurityIncident = 6
}
