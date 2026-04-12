namespace Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;

public sealed record EnforcementRecordedEvent(EnforcementId EnforcementId, EnforcementAction Action);
