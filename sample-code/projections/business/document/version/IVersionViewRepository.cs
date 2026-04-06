namespace Whycespace.Projections.Business.Document.Version;

public interface IVersionViewRepository
{
    Task SaveAsync(VersionReadModel model, CancellationToken ct = default);
    Task<VersionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
