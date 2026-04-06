namespace Whycespace.Platform.Api.Structural.Cluster.Classification;

public sealed record ClassificationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ClassificationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
