namespace Whycespace.Platform.Api.Structural.Humancapital.Stewardship;

public sealed record StewardshipRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record StewardshipResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
