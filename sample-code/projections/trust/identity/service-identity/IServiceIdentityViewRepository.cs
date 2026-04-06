namespace Whycespace.Projections.Trust.Identity.ServiceIdentity;

public interface IServiceIdentityViewRepository
{
    Task SaveAsync(ServiceIdentityReadModel model, CancellationToken ct = default);
    Task<ServiceIdentityReadModel?> GetAsync(string id, CancellationToken ct = default);
}
