namespace Whycespace.Projections.Core.Financialcontrol.VarianceControl;

public interface IVarianceControlViewRepository
{
    Task SaveAsync(VarianceControlReadModel model, CancellationToken ct = default);
    Task<VarianceControlReadModel?> GetAsync(string id, CancellationToken ct = default);
}
