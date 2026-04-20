namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(ProfileStatus status) => status != ProfileStatus.Archived;
}
