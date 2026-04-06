namespace Whycespace.Engines.T2E.Business.Billing.Invoice;

public record InvoiceCommand(
    string Action,
    string EntityId,
    object Payload
);
