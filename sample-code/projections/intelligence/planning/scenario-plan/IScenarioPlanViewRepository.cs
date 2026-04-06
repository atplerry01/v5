namespace Whycespace.Projections.Intelligence.Planning.ScenarioPlan;

public interface IScenarioPlanViewRepository
{
    Task SaveAsync(ScenarioPlanReadModel model, CancellationToken ct = default);
    Task<ScenarioPlanReadModel?> GetAsync(string id, CancellationToken ct = default);
}
