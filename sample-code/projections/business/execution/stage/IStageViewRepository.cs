namespace Whycespace.Projections.Business.Execution.Stage;

public interface IStageViewRepository
{
    Task SaveAsync(StageReadModel model, CancellationToken ct = default);
    Task<StageReadModel?> GetAsync(string id, CancellationToken ct = default);
}
