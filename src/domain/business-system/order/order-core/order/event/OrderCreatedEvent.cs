namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;

public sealed record OrderCreatedEvent(
    OrderId OrderId,
    OrderSourceReference SourceReference,
    string OrderDescription);
