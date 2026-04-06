namespace Whycespace.Platform.Api.Business.Resource.Utilization;

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
