namespace Whycespace.Platform.Api.Intelligence.Index.RegionalIndex;

public sealed record RegionalIndexRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RegionalIndexResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
