namespace Whycespace.Projections.Economic.Revenue.Payout;

public interface IPayoutViewRepository
{
    Task SaveAsync(PayoutReadModel model, CancellationToken ct = default);
    Task<PayoutReadModel?> GetAsync(string id, CancellationToken ct = default);
}
