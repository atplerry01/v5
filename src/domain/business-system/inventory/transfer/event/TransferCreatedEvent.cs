namespace Whycespace.Domain.BusinessSystem.Inventory.Transfer;

public sealed record TransferCreatedEvent(
    TransferId TransferId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    TransferQuantity Quantity);
