namespace Whycespace.Domain.BusinessSystem.Inventory.Movement;

public sealed class IsConfirmedSpecification
{
    public bool IsSatisfiedBy(MovementStatus status)
    {
        return status == MovementStatus.Confirmed;
    }
}
