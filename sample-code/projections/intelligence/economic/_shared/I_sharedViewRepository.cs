namespace Whycespace.Projections.Intelligence.Economic._shared;

public interface I_sharedViewRepository
{
    Task SaveAsync(_sharedReadModel model, CancellationToken ct = default);
    Task<_sharedReadModel?> GetAsync(string id, CancellationToken ct = default);
}
