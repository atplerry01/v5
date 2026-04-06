namespace Whycespace.Platform.Api.Business.Execution.Completion;

public sealed record CompletionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CompletionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
