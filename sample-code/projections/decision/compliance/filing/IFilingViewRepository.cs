namespace Whycespace.Projections.Decision.Compliance.Filing;

public interface IFilingViewRepository
{
    Task SaveAsync(FilingReadModel model, CancellationToken ct = default);
    Task<FilingReadModel?> GetAsync(string id, CancellationToken ct = default);
}
