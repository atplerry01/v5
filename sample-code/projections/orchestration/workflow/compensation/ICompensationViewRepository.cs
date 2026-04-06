namespace Whycespace.Projections.Orchestration.Workflow.Compensation;

public interface ICompensationViewRepository
{
    Task SaveAsync(CompensationReadModel model, CancellationToken ct = default);
    Task<CompensationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
