namespace Whycespace.Projections.Trust.Access.Role;

public interface IRoleViewRepository
{
    Task SaveAsync(RoleReadModel model, CancellationToken ct = default);
    Task<RoleReadModel?> GetAsync(string id, CancellationToken ct = default);
}
