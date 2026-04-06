namespace Whycespace.Projections.Decision.Audit.Access;

public interface IAccessViewRepository
{
    Task SaveAsync(AccessReadModel model, CancellationToken ct = default);
    Task<AccessReadModel?> GetAsync(string id, CancellationToken ct = default);
}
