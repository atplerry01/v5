namespace Whycespace.Platform.Api.Intelligence.Capacity.Supply;

public sealed record SupplyRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SupplyResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
