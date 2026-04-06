namespace Whycespace.Platform.Api.Business.Portfolio.Holding;

public sealed record HoldingRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record HoldingResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
