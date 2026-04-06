namespace Whycespace.Platform.Api.Business.Integration.Callback;

public sealed record CallbackRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CallbackResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
