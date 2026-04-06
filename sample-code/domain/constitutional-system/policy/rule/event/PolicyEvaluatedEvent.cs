using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed record PolicyEvaluatedEvent(Guid PolicyRuleId) : DomainEvent;
