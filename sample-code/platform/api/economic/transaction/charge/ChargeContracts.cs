namespace Whycespace.Platform.Api.Economic.Transaction.Charge;

public sealed record ChargeRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ChargeResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
