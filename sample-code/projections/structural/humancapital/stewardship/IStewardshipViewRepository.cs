namespace Whycespace.Projections.Structural.Humancapital.Stewardship;

public interface IStewardshipViewRepository
{
    Task SaveAsync(StewardshipReadModel model, CancellationToken ct = default);
    Task<StewardshipReadModel?> GetAsync(string id, CancellationToken ct = default);
}
