namespace Whycespace.Projections.Business.Agreement.Counterparty;

public interface ICounterpartyViewRepository
{
    Task SaveAsync(CounterpartyReadModel model, CancellationToken ct = default);
    Task<CounterpartyReadModel?> GetAsync(string id, CancellationToken ct = default);
}
