namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(ConfigurationStatus status) => status != ConfigurationStatus.Archived;
}
