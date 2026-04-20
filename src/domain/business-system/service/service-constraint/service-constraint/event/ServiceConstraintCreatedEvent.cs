using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

public sealed record ServiceConstraintCreatedEvent(
    ServiceConstraintId ServiceConstraintId,
    ServiceDefinitionRef ServiceDefinition,
    ConstraintKind Kind,
    ConstraintDescriptor Descriptor);
