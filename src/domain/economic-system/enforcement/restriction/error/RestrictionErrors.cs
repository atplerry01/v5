using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Restriction;

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
}
