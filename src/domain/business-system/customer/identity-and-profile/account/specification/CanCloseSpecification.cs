namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

public sealed class CanCloseSpecification
{
    public bool IsSatisfiedBy(AccountStatus status) => status != AccountStatus.Closed;
}
