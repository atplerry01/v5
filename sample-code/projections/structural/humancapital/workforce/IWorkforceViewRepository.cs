namespace Whycespace.Projections.Structural.Humancapital.Workforce;

public interface IWorkforceViewRepository
{
    Task SaveAsync(WorkforceReadModel model, CancellationToken ct = default);
    Task<WorkforceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
