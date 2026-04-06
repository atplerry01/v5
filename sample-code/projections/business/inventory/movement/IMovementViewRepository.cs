namespace Whycespace.Projections.Business.Inventory.Movement;

public interface IMovementViewRepository
{
    Task SaveAsync(MovementReadModel model, CancellationToken ct = default);
    Task<MovementReadModel?> GetAsync(string id, CancellationToken ct = default);
}
