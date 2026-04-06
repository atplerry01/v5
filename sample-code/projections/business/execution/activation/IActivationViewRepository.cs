namespace Whycespace.Projections.Business.Execution.Activation;

public interface IActivationViewRepository
{
    Task SaveAsync(ActivationReadModel model, CancellationToken ct = default);
    Task<ActivationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
