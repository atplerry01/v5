namespace Whycespace.Projections.Business.Execution.Charge;

public interface IChargeViewRepository
{
    Task SaveAsync(ChargeReadModel model, CancellationToken ct = default);
    Task<ChargeReadModel?> GetAsync(string id, CancellationToken ct = default);
}
