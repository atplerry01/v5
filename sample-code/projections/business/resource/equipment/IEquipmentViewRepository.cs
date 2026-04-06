namespace Whycespace.Projections.Business.Resource.Equipment;

public interface IEquipmentViewRepository
{
    Task SaveAsync(EquipmentReadModel model, CancellationToken ct = default);
    Task<EquipmentReadModel?> GetAsync(string id, CancellationToken ct = default);
}
