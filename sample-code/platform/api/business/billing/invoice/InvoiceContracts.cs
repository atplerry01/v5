namespace Whycespace.Platform.Api.Business.Billing.Invoice;

public sealed record InvoiceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record InvoiceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
