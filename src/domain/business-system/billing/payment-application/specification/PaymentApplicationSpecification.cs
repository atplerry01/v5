namespace Whycespace.Domain.BusinessSystem.Billing.PaymentApplication;

public sealed class IsFullyAllocatedSpecification
{
    public bool IsSatisfiedBy(decimal totalAllocated, decimal outstandingAmount)
    {
        return totalAllocated >= outstandingAmount;
    }
}
