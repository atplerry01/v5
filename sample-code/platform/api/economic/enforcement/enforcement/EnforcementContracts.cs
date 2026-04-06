namespace Whycespace.Platform.Api.Economic.Enforcement.Enforcement;

public sealed record EnforcementRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record EnforcementResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
