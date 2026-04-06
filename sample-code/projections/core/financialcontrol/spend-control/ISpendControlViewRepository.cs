namespace Whycespace.Projections.Core.Financialcontrol.SpendControl;

public interface ISpendControlViewRepository
{
    Task SaveAsync(SpendControlReadModel model, CancellationToken ct = default);
    Task<SpendControlReadModel?> GetAsync(string id, CancellationToken ct = default);
}
