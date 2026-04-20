namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public sealed class CanCancelSpecification
{
    public bool IsSatisfiedBy(AmendmentStatus status) => status is AmendmentStatus.Requested or AmendmentStatus.Accepted;
}
