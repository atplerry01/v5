namespace Whycespace.Projections.Intelligence.Index.PriceIndex;

public interface IPriceIndexViewRepository
{
    Task SaveAsync(PriceIndexReadModel model, CancellationToken ct = default);
    Task<PriceIndexReadModel?> GetAsync(string id, CancellationToken ct = default);
}
