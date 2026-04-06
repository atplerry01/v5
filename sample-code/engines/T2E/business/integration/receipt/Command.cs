namespace Whycespace.Engines.T2E.Business.Integration.Receipt;

public record ReceiptCommand(
    string Action,
    string EntityId,
    object Payload
);
