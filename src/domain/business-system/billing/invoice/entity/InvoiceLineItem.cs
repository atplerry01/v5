namespace Whycespace.Domain.BusinessSystem.Billing.Invoice;

public sealed class InvoiceLineItem
{
    public Guid LineItemId { get; }
    public string Description { get; }

    public InvoiceLineItem(Guid lineItemId, string description)
    {
        if (lineItemId == Guid.Empty)
            throw new ArgumentException("LineItemId must not be empty.", nameof(lineItemId));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description must not be empty.", nameof(description));

        LineItemId = lineItemId;
        Description = description;
    }
}
