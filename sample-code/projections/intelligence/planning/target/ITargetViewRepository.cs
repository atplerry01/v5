namespace Whycespace.Projections.Intelligence.Planning.Target;

public interface ITargetViewRepository
{
    Task SaveAsync(TargetReadModel model, CancellationToken ct = default);
    Task<TargetReadModel?> GetAsync(string id, CancellationToken ct = default);
}
