namespace Whycespace.Domain.EconomicSystem.Enforcement.Enforcement;

public sealed record EnforcementReleasedEvent(Guid EnforcementId, Guid IdentityId) : DomainEvent;
