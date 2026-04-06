namespace Whycespace.Projections.Intelligence.Economic.Integrity;

public interface IIntegrityViewRepository
{
    Task SaveAsync(IntegrityReadModel model, CancellationToken ct = default);
    Task<IntegrityReadModel?> GetAsync(string id, CancellationToken ct = default);
}
