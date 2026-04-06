namespace Whycespace.Platform.Api.Intelligence.Estimation.DemandSupply;

public sealed record DemandSupplyRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record DemandSupplyResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
