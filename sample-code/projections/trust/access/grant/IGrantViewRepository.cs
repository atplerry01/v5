namespace Whycespace.Projections.Trust.Access.Grant;

public interface IGrantViewRepository
{
    Task SaveAsync(GrantReadModel model, CancellationToken ct = default);
    Task<GrantReadModel?> GetAsync(string id, CancellationToken ct = default);
}
