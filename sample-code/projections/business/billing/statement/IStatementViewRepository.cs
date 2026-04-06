namespace Whycespace.Projections.Business.Billing.Statement;

public interface IStatementViewRepository
{
    Task SaveAsync(StatementReadModel model, CancellationToken ct = default);
    Task<StatementReadModel?> GetAsync(string id, CancellationToken ct = default);
}
