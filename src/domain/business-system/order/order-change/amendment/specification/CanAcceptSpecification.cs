namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public sealed class CanAcceptSpecification
{
    public bool IsSatisfiedBy(AmendmentStatus status) => status == AmendmentStatus.Requested;
}
