namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public sealed class CanRejectSpecification
{
    public bool IsSatisfiedBy(AmendmentStatus status) => status == AmendmentStatus.Requested;
}
