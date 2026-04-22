using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Sanction;

/// <summary>
/// Phase 7 T7.10 added <see cref="Enforcement"/> as an init-only field
/// so V2 streams carry the authoritative link from sanction to the
/// downstream RestrictionId or LockId. V1 streams (serialized without
/// the field) deserialize with <c>Enforcement = null</c>; the
/// aggregate's Apply handler synthesizes a Legacy ref on replay so the
/// invariant "Active sanction must carry an EnforcementRef" stays
/// total without rewriting history.
/// </summary>
public sealed record SanctionActivatedEvent(
    SanctionId SanctionId,
    SubjectId SubjectId,
    Timestamp ActivatedAt) : DomainEvent
{
    public EnforcementRef? Enforcement { get; init; }
}
