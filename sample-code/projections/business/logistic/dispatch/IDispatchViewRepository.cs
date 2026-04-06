namespace Whycespace.Projections.Business.Logistic.Dispatch;

public interface IDispatchViewRepository
{
    Task SaveAsync(DispatchReadModel model, CancellationToken ct = default);
    Task<DispatchReadModel?> GetAsync(string id, CancellationToken ct = default);
}
