namespace Whycespace.Platform.Api.Business.Document.Retention;

public sealed record RetentionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RetentionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
