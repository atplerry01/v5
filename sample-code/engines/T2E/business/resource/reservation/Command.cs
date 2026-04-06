namespace Whycespace.Engines.T2E.Business.Resource.Reservation;

public record ReservationCommand(string Action, string EntityId, object Payload);
public sealed record CreateReservationCommand(string Id) : ReservationCommand("Create", Id, null!);
