namespace Whycespace.Projections.Business.Entitlement.Right;

public interface IRightViewRepository
{
    Task SaveAsync(RightReadModel model, CancellationToken ct = default);
    Task<RightReadModel?> GetAsync(string id, CancellationToken ct = default);
}
