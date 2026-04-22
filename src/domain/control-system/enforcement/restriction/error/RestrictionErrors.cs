using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Restriction;

public static class RestrictionErrors
{
    public static DomainException MissingSubjectReference() =>
        new("Restriction must reference a subject.");

    public static DomainException MissingReason() =>
        new("Restriction must include a reason.");

    public static DomainException RestrictionAlreadyRemoved() =>
        new("Restriction has already been removed.");

    public static DomainException CannotUpdateRemovedRestriction() =>
        new("Cannot update a removed restriction.");

    public static DomainInvariantViolationException EmptyRestrictionId() =>
        new("Invariant violated: RestrictionId cannot be empty.");

    public static DomainInvariantViolationException OrphanRestriction() =>
        new("Invariant violated: restriction must reference a subject.");

    // ── Phase 7 T7.6/T7.7 — cause-coupling + suspend/resume lifecycle ──

    public static DomainException CauseRequired() =>
        new("Restriction requires a non-null EnforcementCause (Phase 7 T7.6).");

    public static DomainException CannotUpdateSuspendedRestriction() =>
        new("Cannot update a suspended restriction — resume it first or remove.");

    public static DomainException CannotSuspendNonAppliedRestriction(RestrictionStatus current) =>
        new($"Restriction can only be suspended from Applied state (was {current}).");

    public static DomainException CannotResumeNonSuspendedRestriction(RestrictionStatus current) =>
        new($"Restriction can only be resumed from Suspended state (was {current}).");

    public static DomainInvariantViolationException CauseMissingOnActiveRestriction() =>
        new("Invariant violated: an Applied or Suspended restriction must carry a non-null Cause.");
}
