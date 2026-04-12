namespace Whycespace.Domain.BusinessSystem.Marketplace.Order;

public sealed record OrderCreatedEvent(
    OrderId OrderId,
    OrderSourceReference SourceReference,
    string OrderDescription);
