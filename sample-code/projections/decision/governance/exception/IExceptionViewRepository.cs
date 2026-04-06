namespace Whycespace.Projections.Decision.Governance.Exception;

public interface IExceptionViewRepository
{
    Task SaveAsync(ExceptionReadModel model, CancellationToken ct = default);
    Task<ExceptionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
