namespace Whycespace.Domain.ConstitutionalSystem.Policy.Constraint;

using Whycespace.Domain.SharedKernel;

public sealed record ConstraintCreatedEvent(
    Guid ConstraintId,
    Guid PolicyRuleId,
    string Name) : DomainEvent;
