namespace Whycespace.Projections.Decision.Risk.Exception;

public interface IExceptionViewRepository
{
    Task SaveAsync(ExceptionReadModel model, CancellationToken ct = default);
    Task<ExceptionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
