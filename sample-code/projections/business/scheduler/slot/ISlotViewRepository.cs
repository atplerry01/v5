namespace Whycespace.Projections.Business.Scheduler.Slot;

public interface ISlotViewRepository
{
    Task SaveAsync(SlotReadModel model, CancellationToken ct = default);
    Task<SlotReadModel?> GetAsync(string id, CancellationToken ct = default);
}
