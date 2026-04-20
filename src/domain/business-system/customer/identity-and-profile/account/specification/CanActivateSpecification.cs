namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(AccountStatus status) => status is AccountStatus.Draft or AccountStatus.Suspended;
}
