namespace Whycespace.Projections.Business.Integration.Callback;

public interface ICallbackViewRepository
{
    Task SaveAsync(CallbackReadModel model, CancellationToken ct = default);
    Task<CallbackReadModel?> GetAsync(string id, CancellationToken ct = default);
}
