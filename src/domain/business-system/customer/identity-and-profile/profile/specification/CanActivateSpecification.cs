namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ProfileStatus status) => status == ProfileStatus.Draft;
}
