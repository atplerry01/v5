namespace Whycespace.Projections.Business.Integration.Endpoint;

public interface IEndpointViewRepository
{
    Task SaveAsync(EndpointReadModel model, CancellationToken ct = default);
    Task<EndpointReadModel?> GetAsync(string id, CancellationToken ct = default);
}
