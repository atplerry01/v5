namespace Whycespace.Projections.Intelligence.Simulation.Model;

public interface IModelViewRepository
{
    Task SaveAsync(ModelReadModel model, CancellationToken ct = default);
    Task<ModelReadModel?> GetAsync(string id, CancellationToken ct = default);
}
