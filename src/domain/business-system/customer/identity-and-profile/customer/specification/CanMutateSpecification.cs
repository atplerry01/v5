namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(CustomerStatus status)
    {
        return status != CustomerStatus.Archived;
    }
}
