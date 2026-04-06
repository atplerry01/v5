namespace Whycespace.Projections.Trust.Access.Permission;

public interface IPermissionViewRepository
{
    Task SaveAsync(PermissionReadModel model, CancellationToken ct = default);
    Task<PermissionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
