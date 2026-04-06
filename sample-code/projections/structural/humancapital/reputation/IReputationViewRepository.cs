namespace Whycespace.Projections.Structural.Humancapital.Reputation;

public interface IReputationViewRepository
{
    Task SaveAsync(ReputationReadModel model, CancellationToken ct = default);
    Task<ReputationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
