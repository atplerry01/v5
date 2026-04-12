namespace Whycespace.Domain.BusinessSystem.Inventory.Lot;

public sealed record LotCreatedEvent(LotId LotId, LotOrigin Origin);
