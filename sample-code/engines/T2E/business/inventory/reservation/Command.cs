namespace Whycespace.Engines.T2E.Business.Inventory.Reservation;

public record ReservationCommand(
    string Action,
    string EntityId,
    object Payload
);
