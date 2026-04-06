namespace Whycespace.Engines.T2E.Business.Logistic.Fulfillment;

public record FulfillmentCommand(
    string Action,
    string EntityId,
    object Payload
);
