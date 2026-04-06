namespace Whycespace.Engines.T2E.Business.Marketplace.Order;

public record OrderCommand(
    string Action,
    string EntityId,
    object Payload
);
