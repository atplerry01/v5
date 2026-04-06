namespace Whycespace.Projections.Business.Portfolio.Portfolio;

public interface IPortfolioViewRepository
{
    Task SaveAsync(PortfolioReadModel model, CancellationToken ct = default);
    Task<PortfolioReadModel?> GetAsync(string id, CancellationToken ct = default);
}
