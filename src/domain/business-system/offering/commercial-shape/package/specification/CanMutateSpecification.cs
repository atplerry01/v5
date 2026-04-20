namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(PackageStatus status) => status != PackageStatus.Archived;
}
