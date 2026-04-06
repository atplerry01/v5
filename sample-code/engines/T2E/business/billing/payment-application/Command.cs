namespace Whycespace.Engines.T2E.Business.Billing.PaymentApplication;

public record PaymentApplicationCommand(
    string Action,
    string EntityId,
    object Payload
);
