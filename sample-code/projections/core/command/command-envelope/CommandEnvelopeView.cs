namespace Whycespace.Projections.Core.Command.CommandEnvelope;

public sealed record CommandEnvelopeView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
