namespace Whycespace.Domain.BusinessSystem.Inventory.Movement;

public sealed class CanConfirmSpecification
{
    public bool IsSatisfiedBy(MovementStatus status)
    {
        return status == MovementStatus.Pending;
    }
}
