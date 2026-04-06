namespace Whycespace.Projections.Orchestration.Workflow.Assignment;

public interface IAssignmentViewRepository
{
    Task SaveAsync(AssignmentReadModel model, CancellationToken ct = default);
    Task<AssignmentReadModel?> GetAsync(string id, CancellationToken ct = default);
}
