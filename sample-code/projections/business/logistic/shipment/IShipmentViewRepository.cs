namespace Whycespace.Projections.Business.Logistic.Shipment;

public interface IShipmentViewRepository
{
    Task SaveAsync(ShipmentReadModel model, CancellationToken ct = default);
    Task<ShipmentReadModel?> GetAsync(string id, CancellationToken ct = default);
}
