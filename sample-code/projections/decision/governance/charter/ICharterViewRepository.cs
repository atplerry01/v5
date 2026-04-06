namespace Whycespace.Projections.Decision.Governance.Charter;

public interface ICharterViewRepository
{
    Task SaveAsync(CharterReadModel model, CancellationToken ct = default);
    Task<CharterReadModel?> GetAsync(string id, CancellationToken ct = default);
}
