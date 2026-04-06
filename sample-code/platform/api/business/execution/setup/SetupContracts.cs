namespace Whycespace.Platform.Api.Business.Execution.Setup;

public sealed record SetupRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SetupResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
