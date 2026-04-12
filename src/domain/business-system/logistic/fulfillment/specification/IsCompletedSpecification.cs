namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

public sealed class IsCompletedSpecification
{
    public bool IsSatisfiedBy(FulfillmentStatus status)
    {
        return status == FulfillmentStatus.Completed;
    }
}
