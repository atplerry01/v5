namespace Whycespace.Domain.BusinessSystem.Inventory.Stock;

public sealed record StockCreatedEvent(StockId StockId, StockItemId ItemId, int InitialQuantity);
