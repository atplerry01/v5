namespace Whycespace.Platform.Api.Business.Resource.Facility;

public sealed record FacilityRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record FacilityResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
