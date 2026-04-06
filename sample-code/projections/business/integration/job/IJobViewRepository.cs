namespace Whycespace.Projections.Business.Integration.Job;

public interface IJobViewRepository
{
    Task SaveAsync(JobReadModel model, CancellationToken ct = default);
    Task<JobReadModel?> GetAsync(string id, CancellationToken ct = default);
}
