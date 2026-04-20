namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(CustomerStatus status)
    {
        return status == CustomerStatus.Draft;
    }
}
