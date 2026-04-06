namespace Whycespace.Platform.Api.Business.Integration.Transformation;

public sealed record TransformationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TransformationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
