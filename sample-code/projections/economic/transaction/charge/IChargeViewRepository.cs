namespace Whycespace.Projections.Economic.Transaction.Charge;

public interface IChargeViewRepository
{
    Task SaveAsync(ChargeReadModel model, CancellationToken ct = default);
    Task<ChargeReadModel?> GetAsync(string id, CancellationToken ct = default);
}
