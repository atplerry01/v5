namespace Whycespace.Projections.Decision.Governance.Appeal;

public interface IAppealViewRepository
{
    Task SaveAsync(AppealReadModel model, CancellationToken ct = default);
    Task<AppealReadModel?> GetAsync(string id, CancellationToken ct = default);
}
