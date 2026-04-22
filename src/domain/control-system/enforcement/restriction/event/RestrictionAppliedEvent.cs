using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Restriction;

/// <summary>
/// Restriction-applied event. Phase 7 T7.6 added <see cref="Cause"/> as
/// an init-only field so V2 streams carry an <see cref="EnforcementCause"/>;
/// V1 streams (serialized without the field) deserialize with
/// <c>Cause = null</c>, and the aggregate's Apply handler synthesizes a
/// Legacy/Manual cause on replay so the non-null Cause invariant stays
/// satisfied without rewriting history.
/// </summary>
public sealed record RestrictionAppliedEvent(
    RestrictionId RestrictionId,
    SubjectId SubjectId,
    RestrictionScope Scope,
    Reason Reason,
    Timestamp AppliedAt) : DomainEvent
{
    public EnforcementCause? Cause { get; init; }
}
