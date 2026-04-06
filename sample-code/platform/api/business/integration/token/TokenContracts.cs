namespace Whycespace.Platform.Api.Business.Integration.Token;

public sealed record TokenRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TokenResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
