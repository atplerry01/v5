namespace Whycespace.Projections.Trust.Access.Request;

public interface IRequestViewRepository
{
    Task SaveAsync(RequestReadModel model, CancellationToken ct = default);
    Task<RequestReadModel?> GetAsync(string id, CancellationToken ct = default);
}
