using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed record PolicyRuleAttachedEvent(
    Guid PolicyId,
    Guid RuleId) : DomainEvent;
