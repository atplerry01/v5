namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public sealed class ReservationWithinInventorySpec
{
    public bool IsSatisfiedBy(decimal reservedAmount, decimal availableInventory)
        => reservedAmount > 0 && reservedAmount <= availableInventory;
}
