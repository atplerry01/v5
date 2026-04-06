namespace Whycespace.Platform.Api.Business.Billing.PaymentApplication;

public sealed record PaymentApplicationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record PaymentApplicationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
