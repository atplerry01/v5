using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Certification;

public sealed class CertificationSpecification : Specification<CertificationStatus>
{
    public override bool IsSatisfiedBy(CertificationStatus entity) =>
        entity == CertificationStatus.Issued || entity == CertificationStatus.Renewed;

    public void EnsureActive(CertificationStatus status)
    {
        if (status == CertificationStatus.Revoked) throw CertificationErrors.AlreadyRevoked();
        if (status == CertificationStatus.Expired) throw CertificationErrors.AlreadyExpired();
    }

    public void EnsureRenewable(CertificationStatus status)
    {
        if (status == CertificationStatus.Revoked) throw CertificationErrors.CannotRenewRevoked();
    }

    public void EnsureValidityWindow(Timestamp from, Timestamp until)
    {
        if (until.Value <= from.Value) throw CertificationErrors.InvalidValidityWindow();
    }
}
