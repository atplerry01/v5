namespace Whycespace.Projections.Core.Event.EventEnvelope;

public sealed record EventEnvelopeView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
