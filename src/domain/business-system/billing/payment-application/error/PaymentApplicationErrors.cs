namespace Whycespace.Domain.BusinessSystem.Billing.PaymentApplication;

public static class PaymentApplicationErrors
{
    public static PaymentApplicationDomainException MissingId()
        => new("PaymentApplicationId is required and must not be empty.");

    public static PaymentApplicationDomainException MissingInvoiceReference()
        => new("Payment application must reference an invoice.");

    public static PaymentApplicationDomainException MissingPaymentSource()
        => new("Payment application must reference a payment source.");

    public static PaymentApplicationDomainException ExceedsOutstandingAmount(decimal applied, decimal outstanding)
        => new($"Applied amount '{applied}' exceeds outstanding amount '{outstanding}'.");

    public static PaymentApplicationDomainException AllocationRequired()
        => new("Payment application must contain at least one allocation.");

    public static PaymentApplicationDomainException InvalidStateTransition(PaymentApplicationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class PaymentApplicationDomainException : Exception
{
    public PaymentApplicationDomainException(string message) : base(message) { }
}
