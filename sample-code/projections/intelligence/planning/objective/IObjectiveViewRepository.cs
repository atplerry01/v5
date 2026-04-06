namespace Whycespace.Projections.Intelligence.Planning.Objective;

public interface IObjectiveViewRepository
{
    Task SaveAsync(ObjectiveReadModel model, CancellationToken ct = default);
    Task<ObjectiveReadModel?> GetAsync(string id, CancellationToken ct = default);
}
