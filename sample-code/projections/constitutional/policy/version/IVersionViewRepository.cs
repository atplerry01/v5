namespace Whycespace.Projections.Constitutional.Policy.Version;

public interface IVersionViewRepository
{
    Task SaveAsync(VersionReadModel model, CancellationToken ct = default);
    Task<VersionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
