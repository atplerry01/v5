namespace Whycespace.Projections.Core.Financialcontrol.BudgetControl;

public interface IBudgetControlViewRepository
{
    Task SaveAsync(BudgetControlReadModel model, CancellationToken ct = default);
    Task<BudgetControlReadModel?> GetAsync(string id, CancellationToken ct = default);
}
