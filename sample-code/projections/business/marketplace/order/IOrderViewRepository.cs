namespace Whycespace.Projections.Business.Marketplace.Order;

public interface IOrderViewRepository
{
    Task SaveAsync(OrderReadModel model, CancellationToken ct = default);
    Task<OrderReadModel?> GetAsync(string id, CancellationToken ct = default);
}
