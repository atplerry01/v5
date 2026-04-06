namespace Whycespace.Projections.Structural.Humancapital.Participant;

public interface IParticipantViewRepository
{
    Task SaveAsync(ParticipantReadModel model, CancellationToken ct = default);
    Task<ParticipantReadModel?> GetAsync(string id, CancellationToken ct = default);
}
