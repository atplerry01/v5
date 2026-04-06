namespace Whycespace.Platform.Api.Business.Billing.Receivable;

public sealed record ReceivableRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ReceivableResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
