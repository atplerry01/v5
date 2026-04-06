namespace Whycespace.Platform.Api.Decision.Risk.Exposure;

public sealed record ExposureRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ExposureResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
