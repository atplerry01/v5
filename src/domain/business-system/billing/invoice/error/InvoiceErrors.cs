namespace Whycespace.Domain.BusinessSystem.Billing.Invoice;

public static class InvoiceErrors
{
    public static InvoiceDomainException MissingId()
        => new("InvoiceId is required and must not be empty.");

    public static InvoiceDomainException LineItemRequired()
        => new("Invoice must contain at least one line item.");

    public static InvoiceDomainException AlreadyIssued(InvoiceId id)
        => new($"Invoice '{id.Value}' has already been issued.");

    public static InvoiceDomainException AlreadyPaid(InvoiceId id)
        => new($"Invoice '{id.Value}' has already been paid.");

    public static InvoiceDomainException AlreadyCancelled(InvoiceId id)
        => new($"Invoice '{id.Value}' has already been cancelled.");

    public static InvoiceDomainException InvalidStateTransition(InvoiceStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class InvoiceDomainException : Exception
{
    public InvoiceDomainException(string message) : base(message) { }
}
