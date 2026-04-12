namespace Whycespace.Domain.BusinessSystem.Inventory.Writeoff;

public sealed record WriteoffCreatedEvent(
    WriteoffId WriteoffId,
    WriteoffReference Reference,
    WriteoffQuantity Quantity,
    WriteoffReason Reason);
