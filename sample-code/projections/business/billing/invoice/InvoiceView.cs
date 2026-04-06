namespace Whycespace.Projections.Business.Billing.Invoice;

public sealed record InvoiceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
