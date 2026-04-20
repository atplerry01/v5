namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public sealed class CanApplySpecification
{
    public bool IsSatisfiedBy(AmendmentStatus status) => status == AmendmentStatus.Accepted;
}
