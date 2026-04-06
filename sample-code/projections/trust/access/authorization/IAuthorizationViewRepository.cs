namespace Whycespace.Projections.Trust.Access.Authorization;

public interface IAuthorizationViewRepository
{
    Task SaveAsync(AuthorizationReadModel model, CancellationToken ct = default);
    Task<AuthorizationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
