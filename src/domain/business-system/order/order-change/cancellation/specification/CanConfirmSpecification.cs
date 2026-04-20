namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

public sealed class CanConfirmSpecification
{
    public bool IsSatisfiedBy(CancellationStatus status) => status == CancellationStatus.Requested;
}
