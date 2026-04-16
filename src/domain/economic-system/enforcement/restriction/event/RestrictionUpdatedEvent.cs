using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Restriction;

public sealed record RestrictionUpdatedEvent(
    RestrictionId RestrictionId,
    SubjectId SubjectId,
    RestrictionScope NewScope,
    Reason NewReason,
    Timestamp UpdatedAt) : DomainEvent;
