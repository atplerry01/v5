namespace Whycespace.Projections.Business.Integration.Client;

public interface IClientViewRepository
{
    Task SaveAsync(ClientReadModel model, CancellationToken ct = default);
    Task<ClientReadModel?> GetAsync(string id, CancellationToken ct = default);
}
