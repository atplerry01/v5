namespace Whycespace.Projections.Business.Agreement.Acceptance;

public interface IAcceptanceViewRepository
{
    Task SaveAsync(AcceptanceReadModel model, CancellationToken ct = default);
    Task<AcceptanceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
