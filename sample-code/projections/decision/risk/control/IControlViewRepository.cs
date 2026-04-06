namespace Whycespace.Projections.Decision.Risk.Control;

public interface IControlViewRepository
{
    Task SaveAsync(ControlReadModel model, CancellationToken ct = default);
    Task<ControlReadModel?> GetAsync(string id, CancellationToken ct = default);
}
