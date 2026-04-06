namespace Whycespace.Platform.Api.Business.Integration.Import;

public sealed record ImportRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ImportResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
