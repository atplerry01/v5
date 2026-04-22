using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Restriction;

/// <summary>
/// Phase 7 T7.7 — restriction temporarily paused for a bounded cause
/// (typically a CompensationFlow against the same subject). The
/// <see cref="SuspensionCause"/> identifies the triggering aggregate so
/// the pause is fully explainable from the event stream. The original
/// <c>Applied</c> cause on the aggregate is preserved — suspension
/// never overwrites it.
/// </summary>
public sealed record RestrictionSuspendedEvent(
    RestrictionId RestrictionId,
    SubjectId SubjectId,
    EnforcementCause SuspensionCause,
    Timestamp SuspendedAt) : DomainEvent;
