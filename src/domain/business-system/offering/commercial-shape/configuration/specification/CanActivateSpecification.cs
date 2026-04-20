namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ConfigurationStatus status) => status == ConfigurationStatus.Draft;
}
