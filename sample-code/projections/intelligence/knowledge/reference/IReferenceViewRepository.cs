namespace Whycespace.Projections.Intelligence.Knowledge.Reference;

public interface IReferenceViewRepository
{
    Task SaveAsync(ReferenceReadModel model, CancellationToken ct = default);
    Task<ReferenceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
