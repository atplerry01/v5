namespace Whycespace.Projections.Business.Notification.Delivery;

public interface IDeliveryViewRepository
{
    Task SaveAsync(DeliveryReadModel model, CancellationToken ct = default);
    Task<DeliveryReadModel?> GetAsync(string id, CancellationToken ct = default);
}
