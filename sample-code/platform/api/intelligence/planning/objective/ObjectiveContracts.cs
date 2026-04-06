namespace Whycespace.Platform.Api.Intelligence.Planning.Objective;

public sealed record ObjectiveRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ObjectiveResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
