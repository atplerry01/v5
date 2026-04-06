namespace Whycespace.Platform.Api.Economic.Revenue.Payout;

public sealed record PayoutRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record PayoutResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
