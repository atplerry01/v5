namespace Whycespace.Platform.Api.Intelligence.Knowledge.Reference;

public sealed record ReferenceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ReferenceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
