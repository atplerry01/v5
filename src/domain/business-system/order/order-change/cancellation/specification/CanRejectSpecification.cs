namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

public sealed class CanRejectSpecification
{
    public bool IsSatisfiedBy(CancellationStatus status) => status == CancellationStatus.Requested;
}
