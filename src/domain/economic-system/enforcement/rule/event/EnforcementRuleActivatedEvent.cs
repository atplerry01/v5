using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Rule;

public sealed record EnforcementRuleActivatedEvent(
    RuleId RuleId) : DomainEvent;
