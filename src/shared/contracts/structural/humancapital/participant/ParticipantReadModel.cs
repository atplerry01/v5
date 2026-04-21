namespace Whycespace.Shared.Contracts.Structural.Humancapital.Participant;

public sealed record ParticipantReadModel
{
    public Guid ParticipantId { get; init; }
    public Guid? HomeClusterId { get; init; }
    public DateTimeOffset? PlacedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
