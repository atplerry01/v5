namespace Whycespace.Projections.Business.Integration.Connector;

public interface IConnectorViewRepository
{
    Task SaveAsync(ConnectorReadModel model, CancellationToken ct = default);
    Task<ConnectorReadModel?> GetAsync(string id, CancellationToken ct = default);
}
