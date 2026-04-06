namespace Whycespace.Domain.ConstitutionalSystem.Policy.Constraint;

using Whycespace.Domain.SharedKernel;

public sealed record ConstraintDeactivatedEvent(Guid ConstraintId) : DomainEvent;
