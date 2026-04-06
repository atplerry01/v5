namespace Whycespace.Platform.Api.Intelligence.Planning.ScenarioPlan;

public sealed record ScenarioPlanRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ScenarioPlanResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
