using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed record PolicyRuleCreatedEvent(Guid PolicyRuleId) : DomainEvent;
