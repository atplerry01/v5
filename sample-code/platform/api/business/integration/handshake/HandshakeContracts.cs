namespace Whycespace.Platform.Api.Business.Integration.Handshake;

public sealed record HandshakeRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record HandshakeResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
