using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Restriction;

public sealed record RestrictionAppliedEvent(
    RestrictionId RestrictionId,
    SubjectId SubjectId,
    RestrictionScope Scope,
    Reason Reason,
    Timestamp AppliedAt) : DomainEvent;
