namespace Whycespace.Platform.Api.Intelligence.Capacity.Utilization;

public sealed record UtilizationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record UtilizationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
