using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Version;

public sealed class AssetVersionSpecification : Specification<AssetVersionStatus>
{
    public override bool IsSatisfiedBy(AssetVersionStatus entity) =>
        entity == AssetVersionStatus.Draft || entity == AssetVersionStatus.Promoted;

    public void EnsurePromotable(AssetVersionStatus status)
    {
        if (status == AssetVersionStatus.Promoted) throw AssetVersionErrors.AlreadyPromoted();
        if (status != AssetVersionStatus.Draft) throw AssetVersionErrors.CannotPromoteNonDraft(status);
    }

    public void EnsureRetirable(AssetVersionStatus status)
    {
        if (status == AssetVersionStatus.Retired) throw AssetVersionErrors.AlreadyRetired();
    }

    public void EnsureSupersedable(AssetVersionStatus status)
    {
        if (status != AssetVersionStatus.Promoted)
            throw AssetVersionErrors.SupersedingNotPromoted();
    }
}
