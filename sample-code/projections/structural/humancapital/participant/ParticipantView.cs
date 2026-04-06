namespace Whycespace.Projections.Structural.Humancapital.Participant;

public sealed record ParticipantView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
