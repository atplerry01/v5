namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(AccountStatus status) => status == AccountStatus.Active;
}
