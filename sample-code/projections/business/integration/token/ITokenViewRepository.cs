namespace Whycespace.Projections.Business.Integration.Token;

public interface ITokenViewRepository
{
    Task SaveAsync(TokenReadModel model, CancellationToken ct = default);
    Task<TokenReadModel?> GetAsync(string id, CancellationToken ct = default);
}
