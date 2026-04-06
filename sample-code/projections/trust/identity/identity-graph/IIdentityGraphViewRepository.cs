namespace Whycespace.Projections.Trust.Identity.IdentityGraph;

public interface IIdentityGraphViewRepository
{
    Task SaveAsync(IdentityGraphReadModel model, CancellationToken ct = default);
    Task<IdentityGraphReadModel?> GetAsync(string id, CancellationToken ct = default);
}
