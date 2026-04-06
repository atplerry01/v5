namespace Whycespace.Projections.CoreSystem.FinancialIntegrity;

public interface IFinancialIntegrityViewRepository
{
    Task SaveAsync(FinancialIntegrityReadModel model, CancellationToken ct = default);
    Task<FinancialIntegrityReadModel?> GetAsync(string id, CancellationToken ct = default);
}
