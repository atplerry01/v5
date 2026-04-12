namespace Whycespace.Domain.BusinessSystem.Subscription.Cancellation;

public static class CanConfirmSpecification
{
    public static bool IsSatisfiedBy(CancellationStatus status) => status == CancellationStatus.Requested;
}
