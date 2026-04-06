namespace Whycespace.Projections.Business.Integration.Handshake;

public sealed record HandshakeView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
