namespace Whycespace.Projections.Structural.Humancapital.Incentive;

public interface IIncentiveViewRepository
{
    Task SaveAsync(IncentiveReadModel model, CancellationToken ct = default);
    Task<IncentiveReadModel?> GetAsync(string id, CancellationToken ct = default);
}
