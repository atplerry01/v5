namespace Whycespace.Projections.Business.Integration.Failure;

public interface IFailureViewRepository
{
    Task SaveAsync(FailureReadModel model, CancellationToken ct = default);
    Task<FailureReadModel?> GetAsync(string id, CancellationToken ct = default);
}
