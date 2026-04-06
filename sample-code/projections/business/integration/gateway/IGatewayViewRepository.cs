namespace Whycespace.Projections.Business.Integration.Gateway;

public interface IGatewayViewRepository
{
    Task SaveAsync(GatewayReadModel model, CancellationToken ct = default);
    Task<GatewayReadModel?> GetAsync(string id, CancellationToken ct = default);
}
