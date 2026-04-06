namespace Whycespace.Projections.Intelligence.Relationship.Linkage;

public interface ILinkageViewRepository
{
    Task SaveAsync(LinkageReadModel model, CancellationToken ct = default);
    Task<LinkageReadModel?> GetAsync(string id, CancellationToken ct = default);
}
