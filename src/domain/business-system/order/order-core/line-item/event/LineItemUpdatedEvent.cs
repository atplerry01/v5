namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public sealed record LineItemUpdatedEvent(LineItemId LineItemId, LineQuantity Quantity);
