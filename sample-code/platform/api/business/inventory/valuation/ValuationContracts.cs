namespace Whycespace.Platform.Api.Business.Inventory.Valuation;

public sealed record ValuationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ValuationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
