namespace Whycespace.Domain.EconomicSystem.Enforcement.Enforcement;

public sealed record EnforcementAppliedEvent(
    Guid EnforcementId,
    Guid IdentityId,
    string EnforcementType,
    string Reason) : DomainEvent;
