namespace Whycespace.Platform.Api.Constitutional.Policy.Version;

public sealed record VersionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record VersionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
