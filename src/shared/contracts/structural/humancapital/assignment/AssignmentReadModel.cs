namespace Whycespace.Shared.Contracts.Structural.Humancapital.Assignment;

public sealed record AssignmentReadModel
{
    public Guid AssignmentId { get; init; }
    public Guid? ParticipantId { get; init; }
    public Guid? AuthorityId { get; init; }
    public DateTimeOffset? EffectiveAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
