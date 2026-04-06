namespace Whycespace.Projections.Business.Execution.Milestone;

public interface IMilestoneViewRepository
{
    Task SaveAsync(MilestoneReadModel model, CancellationToken ct = default);
    Task<MilestoneReadModel?> GetAsync(string id, CancellationToken ct = default);
}
