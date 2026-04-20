namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;

public sealed record OrderCancelledEvent(OrderId OrderId, DateTimeOffset CancelledAt);
