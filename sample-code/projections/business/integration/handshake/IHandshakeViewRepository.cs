namespace Whycespace.Projections.Business.Integration.Handshake;

public interface IHandshakeViewRepository
{
    Task SaveAsync(HandshakeReadModel model, CancellationToken ct = default);
    Task<HandshakeReadModel?> GetAsync(string id, CancellationToken ct = default);
}
