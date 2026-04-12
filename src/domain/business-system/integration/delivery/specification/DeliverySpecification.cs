namespace Whycespace.Domain.BusinessSystem.Integration.Delivery;

public sealed class CanDispatchSpecification
{
    public bool IsSatisfiedBy(DeliveryStatus status) => status == DeliveryStatus.Scheduled;
}

public sealed class CanConfirmSpecification
{
    public bool IsSatisfiedBy(DeliveryStatus status) => status == DeliveryStatus.Dispatched;
}

public sealed class CanFailSpecification
{
    public bool IsSatisfiedBy(DeliveryStatus status) => status == DeliveryStatus.Dispatched;
}
