namespace Whycespace.Projections.Intelligence.Experiment.Variant;

public interface IVariantViewRepository
{
    Task SaveAsync(VariantReadModel model, CancellationToken ct = default);
    Task<VariantReadModel?> GetAsync(string id, CancellationToken ct = default);
}
