namespace Whycespace.Platform.Api.Decision.Audit.Access;

public sealed record AccessRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AccessResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
