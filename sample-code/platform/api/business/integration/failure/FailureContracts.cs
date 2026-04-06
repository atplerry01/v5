namespace Whycespace.Platform.Api.Business.Integration.Failure;

public sealed record FailureRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record FailureResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
