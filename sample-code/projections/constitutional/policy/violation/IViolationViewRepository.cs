namespace Whycespace.Projections.Constitutional.Policy.Violation;

public interface IViolationViewRepository
{
    Task SaveAsync(ViolationReadModel model, CancellationToken ct = default);
    Task<ViolationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
