namespace Whycespace.Projections.Decision.Risk.Mitigation;

public interface IMitigationViewRepository
{
    Task SaveAsync(MitigationReadModel model, CancellationToken ct = default);
    Task<MitigationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
