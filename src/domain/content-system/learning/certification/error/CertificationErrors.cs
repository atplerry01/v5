using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Certification;

public static class CertificationErrors
{
    public static DomainException InvalidSerial() => new("Certificate serial must be non-empty.");
    public static DomainException InvalidHolderRef() => new("Certification holder reference must be non-empty.");
    public static DomainException InvalidCourseRef() => new("Certification course reference must be non-empty.");
    public static DomainException AlreadyRevoked() => new("Certification is already revoked.");
    public static DomainException AlreadyExpired() => new("Certification has already expired.");
    public static DomainException CannotRenewRevoked() => new("A revoked certification cannot be renewed.");
    public static DomainException InvalidValidityWindow() =>
        new("Validity end must be strictly after validity start.");
    public static DomainInvariantViolationException HolderMissing() =>
        new("Invariant violated: certification must have a holder.");
}
