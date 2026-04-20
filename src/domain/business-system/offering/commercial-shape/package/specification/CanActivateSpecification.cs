namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(PackageStatus status, int memberCount)
        => status == PackageStatus.Draft && memberCount > 0;
}
