namespace Whycespace.Projections.Trust.Access.Session;

public interface ISessionViewRepository
{
    Task SaveAsync(SessionReadModel model, CancellationToken ct = default);
    Task<SessionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
