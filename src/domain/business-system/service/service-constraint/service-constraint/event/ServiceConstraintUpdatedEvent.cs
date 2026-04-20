namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

public sealed record ServiceConstraintUpdatedEvent(
    ServiceConstraintId ServiceConstraintId,
    ConstraintKind Kind,
    ConstraintDescriptor Descriptor);
