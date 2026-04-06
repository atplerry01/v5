using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;

public sealed record PolicyEnforcementCreatedEvent(Guid PolicyEnforcementId) : DomainEvent;
