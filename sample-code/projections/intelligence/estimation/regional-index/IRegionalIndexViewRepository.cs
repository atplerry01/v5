namespace Whycespace.Projections.Intelligence.Estimation.RegionalIndex;

public interface IRegionalIndexViewRepository
{
    Task SaveAsync(RegionalIndexReadModel model, CancellationToken ct = default);
    Task<RegionalIndexReadModel?> GetAsync(string id, CancellationToken ct = default);
}
