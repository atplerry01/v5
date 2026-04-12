namespace Whycespace.Domain.BusinessSystem.Inventory.Movement;

public sealed class CanCancelSpecification
{
    public bool IsSatisfiedBy(MovementStatus status)
    {
        return status == MovementStatus.Pending;
    }
}
