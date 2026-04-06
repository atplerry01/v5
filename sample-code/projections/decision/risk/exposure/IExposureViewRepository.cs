namespace Whycespace.Projections.Decision.Risk.Exposure;

public interface IExposureViewRepository
{
    Task SaveAsync(ExposureReadModel model, CancellationToken ct = default);
    Task<ExposureReadModel?> GetAsync(string id, CancellationToken ct = default);
}
