namespace Whycespace.Projections.Structural.Cluster.Authority;

public interface IAuthorityViewRepository
{
    Task SaveAsync(AuthorityReadModel model, CancellationToken ct = default);
    Task<AuthorityReadModel?> GetAsync(string id, CancellationToken ct = default);
}
